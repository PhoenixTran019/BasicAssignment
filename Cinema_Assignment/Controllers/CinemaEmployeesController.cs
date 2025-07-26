using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Cinema_Assignment.Models;

namespace Cinema_Assignment.Controllers
{
    public class CinemaEmployeesController : Controller
    {
        private readonly string _connectionString;
        public CinemaEmployeesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }
        public IActionResult IndexMain()
        {
            var list = new List<EmployeeCinemaViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    Select
                        e.EmployeeID,
                        e.FirstName,
                        e.LastName,
                        e.Roll,
                        c.CinemaName,
                        er.RoleID,
                        er.RoleName,
                        ce.Position,
                        ce.CinemaID
                    From Employees e
                    Left Join Cinemas_Employees ce ON e.EmployeeID = ce.EmployeeID
                    Left Join Cinemas c ON ce.CinemaID = c.CinemaID
                    Left Join EmployeeRoles er ON ce.RoleID = er.RoleID
                    Where e.Roll In (2, 3, 4)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var item = new EmployeeCinemaViewModel
                    {
                        EmployeeID = (int)reader["EmployeeID"],
                        FullName = $"{reader["FirstName"]} {reader["LastName"]}",
                        Roll = (int)reader["Roll"],
                        CinemaName = reader["CinemaName"]?.ToString(),
                        RoleName = reader["RoleName"]?.ToString(),
                        Position = reader["Position"]?.ToString(),
                        CinemaID = reader["CinemaID"] != DBNull.Value ? (int?)reader["CinemaID"] : null
                    };
                    list.Add(item);
                }
            }
                return View(list);
        }

        // GET: Assign - Gán rạp cho nhân viên
        public IActionResult Assign(int employeeId)
        {
            ViewBag.EmployeeID = employeeId;
            ViewBag.Cinemas = GetCinemas();
            ViewBag.EmployeeRoles = GetEmployeeRoles();
            return View();
        }

        //Post: Assign
        [HttpPost]
        public IActionResult Assign(int employeeId, int roleId, int cinemaId, string position, string? otherPosition)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            if (position == "Khác" && string.IsNullOrWhiteSpace(otherPosition))
            {
                TempData["Error"] = "Vui lòng nhập vị trí nếu chọn Khác.";
                return RedirectToAction("Assign", new { employeeId = employeeId });
            }

            string finalPosition = position == "Khác" ? otherPosition : position;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string checkSql = "SELECT COUNT(*) FROM Cinemas_Employees WHERE EmployeeID = @EmployeeID";
                SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                int count = (int)checkCmd.ExecuteScalar();

                string sql = count > 0
                    ? @"UPDATE Cinemas_Employees
                SET RoleID = @RoleID,
                    CinemaID = @CinemaID,
                    Position = @Position
                WHERE EmployeeID = @EmployeeID"
                    : @"INSERT INTO Cinemas_Employees (EmployeeID, RoleID, CinemaID, Position)
                VALUES (@EmployeeID, @RoleID, @CinemaID, @Position)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.Parameters.AddWithValue("@RoleID", roleId);
                cmd.Parameters.AddWithValue("@CinemaID", cinemaId);
                cmd.Parameters.AddWithValue("@Position", finalPosition);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("IndexMain");
        }

        // GET: EditAssign
        public IActionResult EditAssign(int employeeId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            ViewBag.EmployeeID = employeeId;
            ViewBag.Cinemas = GetCinemas();
            ViewBag.EmployeeRoles = GetEmployeeRoles();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Cinemas_Employees WHERE EmployeeID = @EmployeeID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ViewBag.SelectedRoleID = (int)reader["RoleID"];
                    ViewBag.SelectedCinemaID = (int)reader["CinemaID"];
                    ViewBag.SelectedPosition = reader["Position"].ToString();
                }
                else
                {
                    return RedirectToAction("IndexMain");
                }
            }

            return View("EditAssign");
        }

        // POST: EditAssign
        [HttpPost]
        public IActionResult EditAssign(int employeeId, int roleId, int cinemaId, string position, string? otherPosition)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            string finalPosition = position == "Khác" ? otherPosition : position;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
            UPDATE Cinemas_Employees
            SET RoleID = @RoleID,
                CinemaID = @CinemaID,
                Position = @Position
            WHERE EmployeeID = @EmployeeID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.Parameters.AddWithValue("@RoleID", roleId);
                cmd.Parameters.AddWithValue("@CinemaID", cinemaId);
                cmd.Parameters.AddWithValue("@Position", finalPosition);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("IndexMain");
        }

        //Get: DeleteAssign
        [HttpGet]
        public IActionResult DeleteAssign(int employeeId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
            SELECT e.EmployeeID, e.FirstName, e.LastName, r.RoleName, c.CinemaName, ce.Position
            FROM Cinemas_Employees ce
            JOIN Employees e ON ce.EmployeeID = e.EmployeeID
            JOIN EmployeeRoles r ON ce.RoleID = r.RoleID
            JOIN Cinemas c ON ce.CinemaID = c.CinemaID
            WHERE ce.EmployeeID = @EmployeeID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var viewModel = new EmployeeCinemaViewModel
                    {
                        EmployeeID = (int)reader["EmployeeID"],
                        FullName = reader["FirstName"] + " " + reader["LastName"],
                        RoleName = reader["RoleName"].ToString(),
                        CinemaName = reader["CinemaName"].ToString(),
                        Position = reader["Position"].ToString()
                    };
                    return View(viewModel);
                }
            }

            return RedirectToAction("IndexMain");
        }

        //Helper: Get Cinemas List
        private List<CinemaModel> GetCinemas()
        {
            var cinemas = new List<CinemaModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT CinemaID, CinemaName FROM Cinemas";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cinemas.Add(new CinemaModel
                    {
                        CinemaID = (int)reader["CinemaID"],
                        CinemaName = reader["CinemaName"].ToString()
                    });
                }
            }
            return cinemas;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAssignConfirmed(int employeeId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Cinemas_Employees WHERE EmployeeID = @EmployeeID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("IndexMain");
        }

        //Helper: Get Employee Roles
        private List<EmployeeRoleModel> GetEmployeeRoles()
        {
            var roles = new List<EmployeeRoleModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT RoleID, RoleName FROM EmployeeRoles";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    roles.Add(new EmployeeRoleModel
                    {
                        RoleID = (int)reader["RoleID"],
                        RoleName = reader["RoleName"].ToString()
                    });
                }
            }
            return roles;
        }

    }
}
