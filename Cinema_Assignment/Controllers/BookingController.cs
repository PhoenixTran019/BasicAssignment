using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Cinema_Assignment.Models;

namespace Cinema_Assignment.Controllers
{
    public class BookingController : Controller
    {
        private readonly string _connectionString;

        public BookingController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult BookTicket(int showtimeId)
        {
            var vm = new SeatBookingViewModel
            {
                ShowtimeId = showtimeId
            };

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Get movie + showtime info
                var infoCmd = new SqlCommand(@"
                    SELECT s.RoomID, s.StartTime, m.Title
                    FROM Showtimes s
                    JOIN Movies m ON s.MovieID = m.MovieID
                    WHERE s.ShowtimeID = @sid", conn);
                infoCmd.Parameters.AddWithValue("@sid", showtimeId);

                int roomId = 0;
                using (var reader = infoCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        roomId = (int)reader["RoomID"];
                        vm.StartTime = (DateTime)reader["StartTime"];
                        vm.MovieTitle = reader["Title"].ToString();
                    }
                }

                // 2. Load seat types
                var seatTypeCmd = new SqlCommand("SELECT TypeID, TypeName, Price FROM SeatTypes", conn);
                using (var reader = seatTypeCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int typeId = (int)reader["TypeID"];
                        vm.SeatTypes[typeId] = new SeatTypeModel
                        {
                            TypeID = typeId,
                            TypeName = reader["TypeName"].ToString(),
                            Price = (decimal)reader["Price"],
                            Color = typeId == 2 ? "#ffe08a" : "#d0eaff" // hoặc tra từ DB
                        };
                    }
                }

                // 3. Generate seat layout from layout config
                var layoutCmd = new SqlCommand("SELECT StartRow, EndRow, StartCol, EndCol, SeatType FROM SeatLayoutConfigs WHERE RoomID = @roomId", conn);
                layoutCmd.Parameters.AddWithValue("@roomId", roomId);

                using (var reader = layoutCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        char startRow = reader["StartRow"].ToString()[0];
                        char endRow = reader["EndRow"].ToString()[0];
                        int startCol = (int)reader["StartCol"];
                        int endCol = (int)reader["EndCol"];
                        int typeId = (int)reader["SeatType"];

                        for (char row = startRow; row <= endRow; row++)
                        {
                            for (int col = startCol; col <= endCol; col++)
                            {
                                vm.Seats.Add(new TempSeatModel
                                {
                                    Row = row,
                                    Col = col,
                                    TypeID = typeId
                                });
                            }
                        }
                    }
                }

                // 4. Get locked seats from tickets
                var lockedCmd = new SqlCommand("SELECT SeatID FROM Tickets WHERE ShowtimeID = @sid", conn);
                lockedCmd.Parameters.AddWithValue("@sid", showtimeId);

                using (var reader = lockedCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vm.LockedSeatIds.Add(reader["SeatID"].ToString());
                    }
                }
            }

            return View(vm);
        }

        [HttpPost]
        public IActionResult ConfirmBooking(ConfirmBookingRequest model)
        {
            if (string.IsNullOrEmpty(model.SelectedSeats))
            {
                return BadRequest("Bạn chưa chọn ghế.");
            }

            int customerId = GetCurrentCustomerID();
            decimal total = 0m;
            var seatIds = model.SelectedSeats.Split(',').Select(x => x.Trim()).ToList();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();

                try
                {
                    // Tính giá từng ghế
                    var seatPrices = new List<decimal>();
                    foreach (var seatId in seatIds)
                    {
                        var cmd = new SqlCommand(@"
                    SELECT ST.Price
                    FROM SeatLayoutConfigs L
                    JOIN Showtimes S ON L.RoomID = S.RoomID
                    JOIN SeatTypes ST ON L.SeatType = ST.TypeID
                    WHERE S.ShowtimeID = @sid AND
                          CHARINDEX(@row, L.StartRow + L.EndRow) > 0 AND
                          @col BETWEEN L.StartCol AND L.EndCol", conn, tran);

                        cmd.Parameters.AddWithValue("@sid", model.ShowtimeId);
                        cmd.Parameters.AddWithValue("@row", seatId[0].ToString());
                        cmd.Parameters.AddWithValue("@col", int.Parse(seatId.Substring(1)));

                        var price = (decimal?)cmd.ExecuteScalar() ?? 0;
                        total += price;
                        seatPrices.Add(price);
                    }

                    // Insert Payment
                    var paymentCmd = new SqlCommand(@"
                INSERT INTO Payments (Amount, PaymentMethod, PaymentStatus)
                OUTPUT INSERTED.PaymentID
                VALUES (@amount, @method, @status)", conn, tran);

                    paymentCmd.Parameters.AddWithValue("@amount", total);
                    paymentCmd.Parameters.AddWithValue("@method", model.PaymentMethod ?? "Cash");
                    paymentCmd.Parameters.AddWithValue("@status", "Paid");

                    int paymentId = (int)paymentCmd.ExecuteScalar();

                    // Insert Receipt
                    var receiptCmd = new SqlCommand(@"
                INSERT INTO Receipts (CustomerID, EmployeeID, PaymentID, CinemaID, Discount, TotalPrice, FinalAmount, Note)
                OUTPUT INSERTED.ReceiptID
                VALUES (@cust, NULL, @pay, 1, 0, @total, @total, N'Đặt qua website')", conn, tran);

                    receiptCmd.Parameters.AddWithValue("@cust", customerId);
                    receiptCmd.Parameters.AddWithValue("@pay", paymentId);
                    receiptCmd.Parameters.AddWithValue("@total", total);

                    int receiptId = (int)receiptCmd.ExecuteScalar();

                    // Insert Tickets
                    for (int i = 0; i < seatIds.Count; i++)
                    {
                        var ticketCmd = new SqlCommand(@"
                    INSERT INTO Tickets (CustomerID, ShowtimeID, SeatID, ReceiptID, Price)
                    VALUES (@cust, @sid, @seat, @rec, @price)", conn, tran);

                        ticketCmd.Parameters.AddWithValue("@cust", customerId);
                        ticketCmd.Parameters.AddWithValue("@sid", model.ShowtimeId);
                        ticketCmd.Parameters.AddWithValue("@seat", seatIds[i]);
                        ticketCmd.Parameters.AddWithValue("@rec", receiptId);
                        ticketCmd.Parameters.AddWithValue("@price", seatPrices[i]);

                        ticketCmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    return StatusCode(500, "Lỗi khi xử lý thanh toán.");
                }
            }

            return RedirectToAction("Success");
        }

        private int GetCurrentCustomerID()
        {
            var id = HttpContext.Session.GetInt32("CustomerID");
            if (id == null)
                throw new Exception("Chưa đăng nhập");
            return id.Value;
        }
    }
}
