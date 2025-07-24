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
                RoomID = roomId
            };

            // Load Seat Types List
            var seatTypes = new List<SelectListItem>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT TypeID, TypeName FROM SeatTypes", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                seatTypes.Add(new SelectListItem
                {
                    Value = reader["TypeID"].ToString(),
                    Text = reader["TypeName"].ToString()
                });
            }
            ViewBag.SeatTypes = seatTypes;

            return View(model);
        }

        private List<SelectListItem> LoadSeatTypes()
        {
            var list = new List<SelectListItem>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT TypeID, TypeName FROM SeatTypes", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SelectListItem
                {
                    Value = reader["TypeID"].ToString(),
                    Text = reader["TypeName"].ToString()
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
                // Load lại SeatTypes nếu có lỗi để giữ form
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                var seatTypes = new List<SelectListItem>();
                var cmdType = new SqlCommand("SELECT TypeID, TypeName FROM SeatTypes", conn);
                var reader = cmdType.ExecuteReader();
                while (reader.Read())
                {
                    seatTypes.Add(new SelectListItem
                    {
                        Value = reader["TypeID"].ToString(),
                        Text = reader["TypeName"].ToString()
                    });
                }

                ViewBag.SeatTypes = seatTypes;

                return View(model);
            }

            using var connInsert = new SqlConnection(_connectionString);
            connInsert.Open();

            var cmd = new SqlCommand(@"
        INSERT INTO SeatLayoutConfigs
        (LayoutID, RoomID, StartRow, EndRow, StartCol, EndCol, SeatType)
        VALUES (@LayoutID, @RoomID, @StartRow, @EndRow, @StartCol, @EndCol, @SeatType)", connInsert);

            cmd.Parameters.AddWithValue("@LayoutID", model.LayoutID);
            cmd.Parameters.AddWithValue("@RoomID", model.RoomID);
            cmd.Parameters.AddWithValue("@StartRow", model.StartRow);
            cmd.Parameters.AddWithValue("@EndRow", model.EndRow);
            cmd.Parameters.AddWithValue("@StartCol", model.StartCol);
            cmd.Parameters.AddWithValue("@EndCol", model.EndCol);
            cmd.Parameters.AddWithValue("@SeatType", model.SeatType);

            cmd.ExecuteNonQuery();

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

            // Load loại ghế
            var seatTypes = new List<SelectListItem>();
            var seatTypeCmd = new SqlCommand("SELECT TypeID, TypeName FROM SeatTypes", conn);
            var seatTypeReader = seatTypeCmd.ExecuteReader();
            while (seatTypeReader.Read())
            {
                seatTypes.Add(new SelectListItem
                {
                    Value = seatTypeReader["TypeID"].ToString(),
                    Text = seatTypeReader["TypeName"].ToString()
                });
            }

            ViewBag.SeatTypes = seatTypes;

            return View(model);
        }

        [HttpPost]
        public IActionResult EditLayout(SeatLayoutConfigModel model)
        {

            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                Update SeatLayoutConfigs
                Set StartRow = @StartRow,EndRow = @EndRow,
                    StartCol = @StartCol, EndCol = @EndCol,
                    SeatType = @SeatType
                Where LayoutID = @LayoutID", conn);

            cmd.Parameters.AddWithValue("@LayoutID", model.LayoutID);
            cmd.Parameters.AddWithValue("@StartRow", model.StartRow);
            cmd.Parameters.AddWithValue("@EndRow", model.EndRow);
            cmd.Parameters.AddWithValue("@StartCol", model.StartCol);
            cmd.Parameters.AddWithValue("@EndCol", model.EndCol);
            cmd.Parameters.AddWithValue("@SeatType", model.SeatType);

            cmd.ExecuteNonQuery();

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
            {
                TempData["Error"] = "Không tìm thấy Layout cần xoá.";
                return RedirectToAction("Index", "Rooms");
            }

            int roomId = (int)result;

            var cmd = new SqlCommand("DELETE FROM SeatLayoutConfigs WHERE LayoutID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { roomId = roomId });
        }

    }
}
