using Cinema_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Reflection;

namespace Cinema_Assignment.Controllers
{
    public class SeatLayoutConfigController : Controller
    {
        private readonly string _connectionString;
        public SeatLayoutConfigController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        public int GenerateNextLayoutID()
        {
            int nextID = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(MAX(LayoutID), 0) + 1 FROM SeatLayoutConfigs";
                SqlCommand cmd = new SqlCommand(sql, conn);
                nextID = (int)cmd.ExecuteScalar();
            }

            return nextID;
        }

        private int GetCinemaIdByRoom(int roomId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT CinemaID FROM Rooms WHERE RoomID = @RoomID", conn);
            cmd.Parameters.AddWithValue("@RoomID", roomId);

            return (int)cmd.ExecuteScalar();
        }


        public IActionResult Index(int roomId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var list = new List<SeatLayoutConfigModel>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SeatLayoutConfigs WHERE RoomID = @RoomID", conn);
            cmd.Parameters.AddWithValue("@RoomID", roomId);


            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SeatLayoutConfigModel
                {
                    LayoutID = (int)reader["LayoutID"],
                    RoomID = (int)reader["RoomID"],
                    StartRow = reader["StartRow"].ToString()[0], 
                    EndRow = reader["EndRow"].ToString()[0],
                    StartCol = (int)reader["StartCol"],
                    EndCol = (int)reader["EndCol"],
                    SeatType = (int)reader["SeatType"]
                });
            }

            ViewBag.CinemaID = GetCinemaIdByRoom(roomId);
            ViewBag.RoomID = roomId;
            return View(list);
        }

        public IActionResult CreateLayout(int roomId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var model = new SeatLayoutConfigModel
            {
                RoomID = roomId,
                LayoutID = GenerateNextLayoutID(),
            };

            // Load Seat Types List
            var seatTypes = new List<SelectListItem>();
            
            ViewBag.SeatTypes = LoadSeatTypes();

            return View(model);
        }

        private List<SelectListItem> LoadSeatTypes()
        {
            var list = new List<SelectListItem>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT TypeID, Description FROM SeatTypes", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SelectListItem
                {
                    Value = reader["TypeID"].ToString(),
                    Text = reader["Description"].ToString()
                });
            }
            return list;
        }


        [HttpPost]
        public IActionResult CreateLayout(SeatLayoutConfigModel model)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.SeatTypes = LoadSeatTypes();
                return View(model);
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Insert layout
            var cmd = new SqlCommand(@"
                INSERT INTO SeatLayoutConfigs (LayoutID, RoomID, StartRow, EndRow, StartCol, EndCol, SeatType)
                VALUES (@LayoutID, @RoomID, @StartRow, @EndRow, @StartCol, @EndCol, @SeatType)", conn);

            cmd.Parameters.AddWithValue("@LayoutID", model.LayoutID);
            cmd.Parameters.AddWithValue("@RoomID", model.RoomID);
            cmd.Parameters.AddWithValue("@StartRow", model.StartRow);
            cmd.Parameters.AddWithValue("@EndRow", model.EndRow);
            cmd.Parameters.AddWithValue("@StartCol", model.StartCol);
            cmd.Parameters.AddWithValue("@EndCol", model.EndCol);
            cmd.Parameters.AddWithValue("@SeatType", model.SeatType);
            cmd.ExecuteNonQuery();

            // Tạo ghế tương ứng
            for (char row = model.StartRow; row <= model.EndRow; row++)
            {
                for (int col = model.StartCol; col <= model.EndCol; col++)
                {
                    string seatID = $"{model.RoomID}_{row}{col}";
                    string seatName = $"{row}{col}";

                    var seatCmd = new SqlCommand(@"
                        INSERT INTO Seats (SeatID, SeatName, RowChar, ColumNum, RoomID, TypeID, IsLocked)
                        VALUES (@SeatID, @SeatName, @RowChar, @ColumNum, @RoomID, @TypeID, 0)", conn);

                    seatCmd.Parameters.AddWithValue("@SeatID", seatID);
                    seatCmd.Parameters.AddWithValue("@SeatName", seatName);
                    seatCmd.Parameters.AddWithValue("@RowChar", row);
                    seatCmd.Parameters.AddWithValue("@ColumNum", col);
                    seatCmd.Parameters.AddWithValue("@RoomID", model.RoomID);
                    seatCmd.Parameters.AddWithValue("@TypeID", model.SeatType);

                    seatCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index", new { roomId = model.RoomID });
        }

        public IActionResult EditLayout(int layoutId)
        {

            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            SeatLayoutConfigModel model = null;
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SeatLayoutConfigs WHERE LayoutID = @id", conn);
            cmd.Parameters.AddWithValue("@id", layoutId);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model = new SeatLayoutConfigModel
                {
                    LayoutID = (int)reader["LayoutID"],
                    RoomID = (int)reader["RoomID"],
                    StartRow = reader["StartRow"].ToString()[0],
                    EndRow = reader["EndRow"].ToString()[0],
                    StartCol = (int)reader["StartCol"],
                    EndCol = (int)reader["EndCol"],
                    SeatType = (int)reader["SeatType"]
                };
            }
            reader.Close();

            ViewBag.SeatTypes = LoadSeatTypes();

            return View(model);
        }

        [HttpPost]
        public IActionResult EditLayout(SeatLayoutConfigModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Auth");

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Xoá ghế cũ
            var deleteSeats = new SqlCommand("DELETE FROM Seats WHERE RoomID = @RoomID", conn);
            deleteSeats.Parameters.AddWithValue("@RoomID", model.RoomID);
            deleteSeats.ExecuteNonQuery();

            // Cập nhật layout
            var cmd = new SqlCommand(@"
                UPDATE SeatLayoutConfigs
                SET StartRow = @StartRow, EndRow = @EndRow,
                    StartCol = @StartCol, EndCol = @EndCol,
                    SeatType = @SeatType
                WHERE LayoutID = @LayoutID", conn);

            cmd.Parameters.AddWithValue("@LayoutID", model.LayoutID);
            cmd.Parameters.AddWithValue("@StartRow", model.StartRow);
            cmd.Parameters.AddWithValue("@EndRow", model.EndRow);
            cmd.Parameters.AddWithValue("@StartCol", model.StartCol);
            cmd.Parameters.AddWithValue("@EndCol", model.EndCol);
            cmd.Parameters.AddWithValue("@SeatType", model.SeatType);
            cmd.ExecuteNonQuery();

            // Tạo lại ghế
            for (char row = model.StartRow; row <= model.EndRow; row++)
            {
                for (int col = model.StartCol; col <= model.EndCol; col++)
                {
                    string seatID = $"{model.RoomID}_{row}{col}";
                    string seatName = $"{row}{col}";

                    var seatCmd = new SqlCommand(@"
                        INSERT INTO Seats (SeatID, SeatName, RowChar, ColumNum, RoomID, TypeID, IsLocked)
                        VALUES (@SeatID, @SeatName, @RowChar, @ColumNum, @RoomID, @TypeID, 0)", conn);

                    seatCmd.Parameters.AddWithValue("@SeatID", seatID);
                    seatCmd.Parameters.AddWithValue("@SeatName", seatName);
                    seatCmd.Parameters.AddWithValue("@RowChar", row);
                    seatCmd.Parameters.AddWithValue("@ColumNum", col);
                    seatCmd.Parameters.AddWithValue("@RoomID", model.RoomID);
                    seatCmd.Parameters.AddWithValue("@TypeID", model.SeatType);

                    seatCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index", new { roomId = model.RoomID });
        }

        public IActionResult DeleteSeatLayoutConfig (int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var roomCmd = new SqlCommand("SELECT RoomID FROM SeatLayoutConfigs WHERE LayoutID = @id", conn);
            roomCmd.Parameters.AddWithValue("@id", id);
            var result = roomCmd.ExecuteScalar();
            if (result == null)
                return RedirectToAction("Index", "Rooms");

            int roomId = (int)result;

            // Xoá ghế thuộc layout
            var deleteSeats = new SqlCommand("DELETE FROM Seats WHERE RoomID = @RoomID", conn);
            deleteSeats.Parameters.AddWithValue("@RoomID", roomId);
            deleteSeats.ExecuteNonQuery();

            // Xoá layout
            var deleteLayout = new SqlCommand("DELETE FROM SeatLayoutConfigs WHERE LayoutID = @id", conn);
            deleteLayout.Parameters.AddWithValue("@id", id);
            deleteLayout.ExecuteNonQuery();

            return RedirectToAction("Index", new { roomId = roomId });
        }

        public IActionResult ToggleSeatDisable(string seatId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                UPDATE Seats
                SET IsDisabled = IIF(IsDisabled = 1, 0, 1)
                WHERE SeatID = @seatId", conn);
            cmd.Parameters.AddWithValue("@seatId", seatId);
            cmd.ExecuteNonQuery();

            return Ok(); // hoặc Redirect lại layout nếu cần
        }

    }
}
