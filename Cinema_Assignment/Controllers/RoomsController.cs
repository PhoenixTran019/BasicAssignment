using Cinema_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Reflection;

namespace Cinema_Assignment.Controllers
{
    public class RoomsController : Controller
    {
        private readonly string _connectionString;

        public RoomsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        public IActionResult SelectCinema()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            var list= new List<CinemaModel>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("Select CinemaID, CinemaName From Cinemas", conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new CinemaModel
                {
                    CinemaID = (int)reader["CinemaID"],
                    CinemaName = reader["CinemaName"].ToString()
                });
            }

            return View(list);
        }

        public IActionResult RoomOfCinema (int cinemaID)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            var list = new List<RoomsModel>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("Select * From Rooms Where CinemaID = @cinemaID", conn);
            cmd.Parameters.AddWithValue("@cinemaID", cinemaID);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new RoomsModel
                {
                    RoomID = (int)reader["RoomID"],
                    RoomName = reader["RoomName"]?.ToString(),
                    RoomType = reader["RoomType"]?.ToString(),
                    CinemaID = (int)reader["CinemaID"],
                    TotalSeat = (int)reader["TotalSeat"]
                });
            }

            ViewBag.CinemaID = cinemaID;
            return View(list);
        }

        public IActionResult CreateRooms (int cinemaID)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            
            var model = new RoomsModel
            {
                CinemaID = cinemaID
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateRooms (RoomsModel rooms)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                Insert Into Rooms (RoomID, RoomName, RoomType, CinemaID, TotalSeat)
                Values (@RoomID, @Name, @Type, @CinemaID, @TotalSeat)", conn);

            cmd.Parameters.AddWithValue("@RoomID", rooms.RoomID);
            cmd.Parameters.AddWithValue("@Name", rooms.RoomName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Type", rooms.RoomType ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CinemaID", rooms.CinemaID);
            cmd.Parameters.AddWithValue("@TotalSeat", rooms.TotalSeat);

            cmd.ExecuteNonQuery();

            return RedirectToAction("RoomOfCinema", new { cinemaID = rooms.CinemaID });
        }

        public IActionResult EditRooms (int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("Select * From Rooms Where RoomID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var rooms= new RoomsModel
                {
                    RoomID = (int)reader["RoomID"],
                    RoomName = reader["RoomName"]?.ToString(),
                    RoomType = reader["RoomType"]?.ToString(),
                    CinemaID = (int)reader["CinemaID"],
                    TotalSeat = (int)reader["TotalSeat"]
                };
                return View(rooms);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult EditRooms (RoomsModel rooms)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            using var conn =  new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                Update Rooms
                Set RoomName = @Name, RoomType = @Type, TotalSeat = @TotalSeat
                Where RoomID = @ID", conn);

            cmd.Parameters.AddWithValue("@ID", rooms.RoomID);
            cmd.Parameters.AddWithValue("@Name", rooms.RoomName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Type", rooms.RoomType ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TotalSeat", rooms.TotalSeat);

            cmd.ExecuteNonQuery();
            return RedirectToAction("RoomOfCinema", new { cinemaID = rooms.CinemaID });
        }

        public IActionResult DeleteRooms(int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            int cinemaID = GetCinemaIdByRoom(id);

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("Delete From Rooms Where RoomID = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("RoomOfCinema", new { cinemaID = cinemaID });
        }

        private int GetCinemaIdByRoom(int roomId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("Select CinemaID From Rooms Where RoomID = @RoomID", conn);
            cmd.Parameters.AddWithValue("@RoomID", roomId);

            return (int)cmd.ExecuteScalar();
        }

    }
}
