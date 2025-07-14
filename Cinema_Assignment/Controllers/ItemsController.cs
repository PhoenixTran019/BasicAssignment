using Cinema_Assignment.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace Cinema_Assignment.Controllers
{
    public class ItemsController : Controller
    {
        private readonly string _connectionString;

        public ItemsController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        public IActionResult Index(ItemModel item)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var items = new List<ItemModel>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Items", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new ItemModel
                {
                    ItemID = (int)reader["ItemID"],
                    ItemName = reader["ItemName"].ToString(),
                    Unit = reader["Unit"].ToString(),
                    QuanlityPerUnit = (int)reader["QuanlityPerUnit"],
                    Category = reader["Category"].ToString(),
                    Description = reader["Description"].ToString()
                });
            }
            return View(items);
        }

        public IActionResult CreateItems()
        {
            ViewBag.UnitList = new SelectList(new List<string>
    {
        "Cái", "Hộp", "Kg", "Thùng", "Chai"
    });

            ViewBag.CategoryList = new SelectList(new List<string>
    {
        "Thực phẩm", "Đồ gia dụng", "Đồ uống", "Thiết bị"
    });

            return View();
        }



        [HttpGet]
        public IActionResult EditItems(int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM Items WHERE ItemID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();

            ItemModel item = null;
            if (reader.Read())
            {
                item = new ItemModel
                {
                    ItemID = (int)reader["ItemID"],
                    ItemName = reader["ItemName"]?.ToString(),
                    Unit = reader["Unit"]?.ToString(),
                    QuanlityPerUnit = reader["QuanlityPerUnit"] != DBNull.Value ? Convert.ToInt32(reader["QuanlityPerUnit"]) : 0,
                    Category = reader["Category"]?.ToString(),
                    Description = reader["Description"]?.ToString()
                };
            }

            reader.Close();

            if (item == null)
                return NotFound();

            return View(item); // Trả về view để sửa
        }


        [HttpPost]
        public IActionResult EditItems(ItemModel item)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
        UPDATE Items 
        SET ItemName = @Name, Unit = @Unit, QuanlityPerUnit = @QtyPerUnit, 
            Category = @Category, Description = @Description
        WHERE ItemID = @id", conn);

            cmd.Parameters.AddWithValue("@id", item.ItemID);
            cmd.Parameters.AddWithValue("@Name", string.IsNullOrWhiteSpace(item.ItemName) ? "" : item.ItemName);
            cmd.Parameters.AddWithValue("@Unit", string.IsNullOrWhiteSpace(item.Unit) ? "" : item.Unit);
            cmd.Parameters.AddWithValue("@QtyPerUnit", item.QuanlityPerUnit);
            cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(item.Category) ? "" : item.Category);
            cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(item.Description) ? "" : item.Description);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }



        public IActionResult DeleteItems (int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var checkStockCmd = new SqlCommand("SELECT COUNT(*) FROM Cinemas_ItemsStock WHERE ItemID = @id AND Quantity > 0", conn);
            checkStockCmd.Parameters.AddWithValue("@id", id);
            int stockCount = (int)checkStockCmd.ExecuteScalar();

            if (stockCount > 0)
            {
                TempData["Error"] = "Không thể xoá mặt hàng vì vẫn còn tồn tại trong kho của các rạp.";
                return RedirectToAction("Index");
            }

            var cmd = new SqlCommand("DELETE FROM Items WHERE ItemID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");

        }

        public IActionResult DetailItems (int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Items WHERE ItemID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var item = new ItemModel
                {
                    ItemID = (int)reader["ItemID"],
                    ItemName = reader["ItemName"].ToString(),
                    Unit = reader["Unit"].ToString(),
                    QuanlityPerUnit = (int)reader["QuanlityPerUnit"],
                    Category = reader["Category"].ToString(),
                    Description = reader["Description"].ToString()
                };
                return View(item);
            }
            return NotFound();
        }

        public IActionResult SelectCinema()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var cinemas = new List<SelectListItem>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT CinemaID, CinemaName FROM Cinemas", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cinemas.Add(new SelectListItem
                {
                    Value = reader["CinemaID"].ToString(),
                    Text = reader["CinemaName"].ToString()
                });
            }
            ViewBag.CinemaList = cinemas;
            return View();
        }

        public IActionResult StockByCinema(int cinemaId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var items = new List<ItemStockViewModel>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
        SELECT i.ItemID, i.ItemName, i.Unit, i.QuanlityPerUnit, i.Category, i.Description, s.IsActive, s.Quantity
        FROM Items i
        INNER JOIN Cinemas_ItemsStock s ON i.ItemID = s.ItemID
        WHERE s.CinemaID = @cinemaId", conn);

            cmd.Parameters.AddWithValue("@cinemaId", cinemaId);

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new ItemStockViewModel
                {
                    ItemID = (int)reader["ItemID"],
                    ItemName = reader["ItemName"].ToString(),
                    Unit = reader["Unit"].ToString(),
                    QuanlityPerUnit = (int)reader["QuanlityPerUnit"],
                    Category = reader["Category"].ToString(),
                    Description = reader["Description"].ToString(),
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    Quantity = (int)reader["Quantity"]
                });
            }

            return View(items);
        }

    }
}
