using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using Cinema_Assignment.Models;

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
                    StartRow = (char)reader["StartRow"],
                    EndRow = (char)reader["EndRow"],
                    StartCol = (int)reader["StartCol"],
                    EndCol = (int)reader["EndCol"],
                    SeatTypeID = (int)reader["SeatTypeID"]
                });
            }

            ViewBag.RoomID = roomId;
            return View(list);
        }

        public IActionResult CreateSeatLayoutConfig(int roomId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.RoomID = roomId;
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
            return View();
        }

        [HttpPost]
        public IActionResult CreatSeatLayoutConfig (SeatLayoutConfigModel layout)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                Insert Into SeatLayoutConfigs
                (LayoutID, RoomID, StartRow, EndRow, StartCol, EndCol, SeatType)
                Values (@LayoutID, @RoomID, @StartRow, @EndRow, @StartCol, @EndCol, @SeatType)", conn);

            cmd.Parameters.AddWithValue("@LayoutID", layout.LayoutID);
            cmd.Parameters.AddWithValue("@RoomID", layout.RoomID);
            cmd.Parameters.AddWithValue("@StartRow", layout.StartRow);
            cmd.Parameters.AddWithValue("@EndRow", layout.EndRow);
            cmd.Parameters.AddWithValue("@StartCol", layout.StartCol);
            cmd.Parameters.AddWithValue("@EndCol", layout.EndCol);
            cmd.Parameters.AddWithValue("@SeatType", layout.SeatTypeID);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { roomId = layout.RoomID });
        }

        public IActionResult EditSeatLayoutConfig (int id)
        {

            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("Select *From SeatLayoutConfigs Where LayoutID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var layout = new SeatLayoutConfigModel
                {
                    LayoutID = (int)reader["LayoutID"],
                    RoomID = (int)reader["RoomID"],
                    StartRow = (char)reader["StartRow"],
                    EndRow = (char)reader["EndRow"],
                    StartCol = (int)reader["StartCol"],
                    EndCol = (int)reader["EndCol"],
                    SeatTypeID = (int)reader["SeatTypeID"]
                };

                //Load Seat Types List
                var seatTypes = new List<SelectListItem>();
                reader.Close();
                var seatCmd = new SqlCommand ("Select TypeID, TypeName From SeatTypes", conn);
                var seatReader = seatCmd.ExecuteReader();
                while (seatReader.Read())
                {
                    seatTypes.Add(new SelectListItem
                    {
                        Value = seatReader["TypeID"].ToString(),
                        Text = seatReader["TypeName"].ToString()
                    });
                }
                ViewBag.SeatTypes = seatTypes;

                return View(layout);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult EditSeatLayoutConfig (SeatLayoutConfigModel layout)
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
                    SeatTypeID = @SeatTypeID
                Where LayoutID = @LayoutID", conn);

            cmd.Parameters.AddWithValue("@LayoutID", layout.LayoutID);
            cmd.Parameters.AddWithValue("@StartRow", layout.StartRow);
            cmd.Parameters.AddWithValue("@EndRow", layout.EndRow);
            cmd.Parameters.AddWithValue("@StartCol", layout.StartCol);
            cmd.Parameters.AddWithValue("@EndCol", layout.EndCol);
            cmd.Parameters.AddWithValue("@SeatTypeID", layout.SeatTypeID);

            cmd.ExecuteNonQuery();
            return RedirectToAction("Index", new { roomId = layout.RoomID });
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

            //Take RoomID to back into the room After deletion
            var roomCmd = new SqlCommand("Select RoomID From SeatLayoutConfigs Where LayoutID = @id", conn);
            roomCmd.Parameters.AddWithValue("@id", id);
            int roomId = (int)roomCmd.ExecuteScalar();

            var cmd = new SqlCommand ("Delete From SeatLayoutConfigs Where LayoutID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { roomId = roomId });
        }

    }
}
