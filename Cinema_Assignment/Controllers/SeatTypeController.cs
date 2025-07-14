using Cinema_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Cinema_Assignment.Controllers
{
    public class SeatTypeController : Controller
    {

        private readonly string _connectionString;

        public SeatTypeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var list = new List<SeatTypeModel>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SeatTypes", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SeatTypeModel
                {
                    TypeID = (int)reader["TypeID"],
                    TypeName = reader["TypeName"].ToString(),
                    Price = (decimal)reader["Price"],
                    Description = reader["Description"]?.ToString() ?? ""
                });
            }

            return View(list);
        }

        public IActionResult CreateSeatType() => View();

        [HttpPost]
        public IActionResult CreateSeatType (SeatTypeModel seatType)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                Insert Into SeatTypes (TypeID,TypeName, Price, Description)
                Values (@ID, @Name, @Price, @Descrp)", conn);

            cmd.Parameters.AddWithValue("@ID", seatType.TypeID);
            cmd.Parameters.AddWithValue("@Name", seatType.TypeName??"");
            cmd.Parameters.AddWithValue("@Price", seatType.Price);
            cmd.Parameters.AddWithValue("@Descrp", seatType.Description ?? "");

            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        public IActionResult EditSeatType(int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM SeatTypes WHERE TypeID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var seatType = new SeatTypeModel
                {
                    TypeID = (int)reader["TypeID"],
                    TypeName = reader["TypeName"].ToString(),
                    Price = (decimal)reader["Price"],
                    Description = reader["Description"].ToString()
                };
                return View(seatType);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult EditSeatType (SeatTypeModel seatType)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                UPDATE SeatTypes 
                SET TypeName = @Name, Price = @Price, Description = @Descrp 
                WHERE TypeID = @ID", conn);

            cmd.Parameters.AddWithValue("@ID", seatType.TypeID);
            cmd.Parameters.AddWithValue("@Name", seatType.TypeName ?? "");
            cmd.Parameters.AddWithValue("@Price", seatType.Price);
            cmd.Parameters.AddWithValue("@Descrp", seatType.Description ?? "");

            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        public IActionResult DeleteSeatType(int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM SeatTypes WHERE TypeID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
