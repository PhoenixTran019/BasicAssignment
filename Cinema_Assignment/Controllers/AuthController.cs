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
                    Select e.EmployeeID, e.Roll
                    From AccountEmployee ae
                    join Employee e on ae.EmployeeID = e.EmployeeID
                    where ae.Username = @Username and ae.Password = @Password", conn);

                cmd.Parameters.AddWithValue("@username", model.Username);
                cmd.Parameters.AddWithValue("@password", hash);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    HttpContext.Session.SetString ("UserType", "Employee");
                    HttpContext.Session.SetInt32("UserID", (int)reader["EmployeeID"]);

                    int role = (int)reader["Roll"];
                    return role switch
                    {
                        1 => RedirectToAction("Index", "Admin"),
                        2 => RedirectToAction("Index", "Cinema"),
                        3 => RedirectToAction("Index", "TeamLead"),
                        4 => RedirectToAction("Index", "Employee")
                    };
                }
            }
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
