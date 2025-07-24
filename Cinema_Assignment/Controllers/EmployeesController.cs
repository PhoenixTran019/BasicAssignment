using Microsoft.AspNetCore.Mvc;
using Cinema_Assignment.Models;
using System.Data.SqlClient;

namespace Cinema_Assignment.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly string connectionString;

        public EmployeesController(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }

        bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        // Hàm kiểm tra trùng EmployeeID
        private bool IsEmployeeIDExists(int employeeID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Employees WHERE EmployeeID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", employeeID);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            var list = new List<EmployeesModel>();
            
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var sql = "SELECT * FROM Employees";
                var cmd = new SqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new EmployeesModel
                    {
                        EmployeeId = (int)reader["EmployeeId"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DOB = (DateTime)reader["DOB"],
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Roll = (int)reader["Roll"],
                        Detail = reader["Detail"].ToString(),
                        Address = reader["Address"].ToString(),
                        JobAcceptanceDate = (DateTime)reader["JobAcceptanceDate"]
                    });
                }
            }
            return View(list);
        }
        public IActionResult CreateEmployees()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        // Post: Employees/Create
        [HttpPost]
        public IActionResult CreateEmployees(EmployeesModel emp)
        {
            if(!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            if (IsEmployeeIDExists(emp.EmployeeId))
            {
                ModelState.AddModelError("EmployeeID", "❌ Mã nhân viên đã tồn tại.");
                return View(emp);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    Insert Into Employees
                    (EmployeeID, FirstName, LastName,Gender, DOB, Email, PhoneNumber, Roll, Detail, Address)
                    Values
                    (@EmployeeID, @FirstName, @LastName, @Gender, @DOB, @Email, @PhoneNumber, @Roll, @Detail, @Address)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeID", emp.EmployeeId);
                cmd.Parameters.AddWithValue("@FirstName", emp.FirstName);
                cmd.Parameters.AddWithValue("@LastName", emp.LastName);
                cmd.Parameters.AddWithValue("@Gender", emp.Gender);
                cmd.Parameters.AddWithValue("@DOB", emp.DOB);
                cmd.Parameters.AddWithValue("@Email", emp.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", emp.PhoneNumber);
                cmd.Parameters.AddWithValue("@Roll", emp.Roll);
                cmd.Parameters.AddWithValue("@Detail", emp.Detail);
                cmd.Parameters.AddWithValue("@Address", emp.Address);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        //Get: Employees/Edit/{id}
        public IActionResult EditEmployees (int id)
        {
            if(!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            EmployeesModel emp = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "Select * From Employees Where EmployeeID = @EmpId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmpId", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    emp = new EmployeesModel
                    {
                        EmployeeId = (int)reader["EmployeeId"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DOB = Convert.ToDateTime(reader["DOB"]),
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Roll = Convert.ToInt32(reader["Roll"]),
                        Detail = reader["Detail"].ToString(),
                        Address = reader["Address"].ToString()
                    };
                }
            }
            if (emp == null)
            {
                return NotFound();
            }
            return View(emp);
        }

        //Post: Employees/Edit/{id}
        [HttpPost]
        public IActionResult EditEmployees (EmployeesModel emp)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    Update Employees Set
                    FirstName = @FirstName, LastName = @LastName, Gender = @Gender,
                    DOB= @DOB, Email = @Email, PhoneNumber = @PhoneNumber, Roll = @Roll,
                    Detail = @Detail, Address = @Address
                    Where EmployeeID = @EmployeeID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FirstName", emp.FirstName);
                cmd.Parameters.AddWithValue("@LastName", emp.LastName);
                cmd.Parameters.AddWithValue("@Gender", emp.Gender);
                cmd.Parameters.AddWithValue("@DOB", emp.DOB);
                cmd.Parameters.AddWithValue("@Email", emp.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", emp.PhoneNumber);
                cmd.Parameters.AddWithValue("@Roll", emp.Roll);
                cmd.Parameters.AddWithValue("@Detail", emp.Detail);
                cmd.Parameters.AddWithValue("@Address", emp.Address);
                cmd.Parameters.AddWithValue("@EmployeeID", emp.EmployeeId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public IActionResult DetailEmployees (string id)
        {
            EmployeesModel emp = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Employees WHERE EmployeeID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    emp = new EmployeesModel
                    {
                        EmployeeId = (int)reader["EmployeeId"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DOB = (DateTime)reader["DOB"],
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Roll = Convert.ToInt32(reader["Roll"]),
                        Detail = reader["Detail"].ToString(),
                        Address = reader["Address"].ToString(),
                        JobAcceptanceDate = (DateTime)reader["JobAcceptanceDate"]
                    };
                }
            }

            if (emp == null) return NotFound();

            return View(emp);
        }


        public IActionResult Delete(string id)
        {
            EmployeesModel emp = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Employees WHERE EmployeeID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    emp = new EmployeesModel
                    {
                        EmployeeId = (int)reader["EmployeeId"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        Email = reader["Email"].ToString()
                        // thêm các trường nếu cần
                    };
                }
            }

            if (emp == null)
                return NotFound();

            return View(emp);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Employees WHERE EmployeeID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }


    }
}
