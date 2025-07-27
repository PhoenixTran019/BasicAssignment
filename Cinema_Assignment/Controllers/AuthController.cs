using Cinema_Assignment.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cinema_Assignment.Controllers
{
    public class AuthController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

        

        public AuthController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _env = env;
        }

        //===Login===
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login (LoginViewModel model)
        {
            string hash = ComputeSha256Hash(model.Password);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                //Employee login
                var cmd = new SqlCommand(@"
                    SELECT e.EmployeeID, e.Roll
                    FROM AccountEmployees ae
                    JOIN Employees e ON ae.EmployeeID = e.EmployeeID
                    WHERE ae.Username = @username AND ae.PasswordHash = @password", conn);

                cmd.Parameters.AddWithValue("@username", model.Identifier);
                cmd.Parameters.AddWithValue("@password", hash);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int roll = (int)reader["Roll"];
                    HttpContext.Session.SetInt32("UserRoll", roll);
                    HttpContext.Session.SetString ("UserType", "Employee");
                    HttpContext.Session.SetInt32("UserID", (int)reader["EmployeeID"]);

                    

                    return roll switch
                    {
                        1 => RedirectToAction("Home", "Admin"),
                        2 => RedirectToAction("Index", "Cinema"),
                        3 => RedirectToAction("Index", "TeamLead"),
                        4 => RedirectToAction("Index", "Employee")
                    };
                }
                reader.Close();
                var cmd2 = new SqlCommand(@"
            SELECT ac.CustomerID
            FROM LoginIdentifiers li
            JOIN AccountCustomers ac ON li.AccountID = ac.AccountID
            WHERE li.Identifier = @identifier AND ac.PasswordHash = @password", conn);

                cmd2.Parameters.AddWithValue("@identifier", model.Identifier);
                cmd2.Parameters.AddWithValue("@password", hash);

                var reader2 = cmd2.ExecuteReader();
                if (reader2.Read())
                {
                    HttpContext.Session.SetString("UserType", "Customer");
                    HttpContext.Session.SetInt32("UserID", (int)reader2["CustomerID"]);
                    return RedirectToAction("Home", "Customer"); // hoặc trang phim
                }

            }
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ session
            return RedirectToAction("Index", "Home"); // ← Chuyển về trang Home
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
