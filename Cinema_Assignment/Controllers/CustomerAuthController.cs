using Cinema_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Cinema_Assignment.Controllers
{
    public class CustomerAuthController : Controller
    {
        private readonly string connectionString;

        public CustomerAuthController(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }

        public int GenerateNextCustomerID()
        {
            int nextID = 1;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(MAX(CustomerID), 0) + 1 FROM Customers";
                SqlCommand cmd = new SqlCommand(sql, conn);
                nextID = (int)cmd.ExecuteScalar();
            }

            return nextID;
        }

        public int GenerateNextAccountID()
        {
            int nextID = 1;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(MAX(AccountID), 0) + 1 FROM AccountCustomers";
                SqlCommand cmd = new SqlCommand(sql, conn);
                nextID = (int)cmd.ExecuteScalar();
            }

            return nextID;
        }

        

        [HttpGet]
        public IActionResult Register()
        {
            var customer = new CustomerRegisterViewModel
            {
                 CustomerID = GenerateNextCustomerID(),
                 DOB = DateTime.Now.Date,
                 AccountID = GenerateNextAccountID(),
            };
            return View(customer);
        }

        [HttpPost]
        public IActionResult Register(CustomerRegisterViewModel model)
        {
            if (!ModelState.IsValid || model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Thông tin không hợp lệ hoặc mật khẩu xác nhận không khớp.");
                return View(model);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Kiểm tra trùng SĐT hoặc Email trong LoginIdentifiers
                var checkCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM LoginIdentifiers 
                WHERE Identifier = @Email OR Identifier = @Phone", conn);
                checkCmd.Parameters.AddWithValue("@Email", model.Email);
                checkCmd.Parameters.AddWithValue("@Phone", model.PhoneNumber);

                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    ModelState.AddModelError("", "Email hoặc số điện thoại đã tồn tại.");
                    return View(model);
                }

                // Thêm vào Customers
                var insertCustomer = new SqlCommand(@"
                INSERT INTO Customers (CustomerID,FirstName, LastName, Gender, DOB, PhoneNumber, Email, Address)
                OUTPUT INSERTED.CustomerID
                VALUES (@CusID,@FirstName, @LastName, @Gender, @DOB, @Phone, @Email, @Address)", conn);

                insertCustomer.Parameters.AddWithValue("@CusID", model.CustomerID);
                insertCustomer.Parameters.AddWithValue("@FirstName", model.FirstName);
                insertCustomer.Parameters.AddWithValue("@LastName", model.LastName);
                insertCustomer.Parameters.AddWithValue("@Gender", model.Gender);
                insertCustomer.Parameters.AddWithValue("@DOB", model.DOB);
                insertCustomer.Parameters.AddWithValue("@Phone", model.PhoneNumber);
                insertCustomer.Parameters.AddWithValue("@Email", model.Email);
                insertCustomer.Parameters.AddWithValue("@Address", model.Address);

                int customerId = (int)insertCustomer.ExecuteScalar();

                // Hash mật khẩu
                string hash = ComputeSha256Hash(model.Password);

                // Thêm vào AccountCustomers
                var insertAccount = new SqlCommand(@"
                INSERT INTO AccountCustomers (AccountID,PasswordHash, CustomerID)
                OUTPUT INSERTED.AccountID
                VALUES (@AccID,@Password, @CustomerID)", conn);

                insertAccount.Parameters.AddWithValue("@AccID", model.AccountID);
                insertAccount.Parameters.AddWithValue("@Password", hash);
                insertAccount.Parameters.AddWithValue("@CustomerID", customerId);
                int accountId = (int)insertAccount.ExecuteScalar();

                // Thêm định danh SĐT & Email
                var insertPhone = new SqlCommand(@"
                INSERT INTO LoginIdentifiers (AccountID, Identifier, Type, IsPrimary)
                VALUES ( @AccountID, @Phone, 'Phone', 1)", conn);
                
                insertPhone.Parameters.AddWithValue("@AccountID", accountId);
                insertPhone.Parameters.AddWithValue("@Phone", model.PhoneNumber);
                insertPhone.ExecuteNonQuery();

                var insertEmail = new SqlCommand(@"
                INSERT INTO LoginIdentifiers (AccountID, Identifier, Type, IsPrimary)
                VALUES (@AccountID, @Email, 'Email', 0)", conn);
                insertEmail.Parameters.AddWithValue("@AccountID", accountId);
                insertEmail.Parameters.AddWithValue("@Email", model.Email);
                insertEmail.ExecuteNonQuery();

                // Đăng nhập ngay sau khi đăng ký (tuỳ chọn)
                HttpContext.Session.SetInt32("UserID", customerId);
                HttpContext.Session.SetString("UserType", "Customer");

                return RedirectToAction("Home", "Customer");
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

    }
}
