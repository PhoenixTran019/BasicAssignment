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

        public IActionResult Index(int? cinemaId)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            
                var items = new List<ItemModel>();
                var cinemaList = new List<SelectListItem>();
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                // Lấy danh sách rạp
                var cinemaCmd = new SqlCommand("SELECT CinemaID, CinemaName FROM Cinemas", conn);
                var reader = cinemaCmd.ExecuteReader();
                while (reader.Read())
                {
                    cinemaList.Add(new SelectListItem
                    {
                        Value = reader["CinemaID"].ToString(),
                        Text = reader["CinemaName"].ToString(),
                        Selected = (cinemaId != null && cinemaId == (int)reader["CinemaID"])
                    });
                }
                reader.Close();

                ViewBag.CinemaList = cinemaList;

                if (cinemaId == null)
                    return View(items);
                conn.Open();
                var cmd = new SqlCommand(@"SELECT i.ItemID, i.ItemName, i.Unit, i.QuanlityPerUnit, i.Category, i.Description, s.Quantity FROM Items i INNER JOIN Cinemas_ItemsStock s ON i.ItemID = s.ItemID
                                            WHERE s.CinemaID = @cinemaId", conn);
                cmd.Parameters.AddWithValue("@cinemaId", cinemaId);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new ItemModel
                    {
                        ItemID = (int)reader["ItemID"],
                        ItemName = reader["ItemName"].ToString(),
                        Unit = reader["Unit"].ToString(),
                        QuanlityPerUnit = (int)reader["QuanlityPerUnit"],
                        Category = reader["Category"].ToString(),
                        Description = reader["Description"].ToString() + $" (Còn lại: {reader["Quantity"]})"
                    });
                }
                return View(items);
            }

        public IActionResult CreateItem()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateItem (ItemModel item)
        {
            
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            int newItemId = 0;

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
            INSERT INTO Items (ItemName, Unit, QuanlityPerUnit, Category, Description)
            OUTPUT INSERTED.ItemID
            VALUES (@Name, @Unit, @QtyPerUnit, @Category, @Description)", conn);

            cmd.Parameters.AddWithValue("@Name", item.ItemName);
            cmd.Parameters.AddWithValue("@Unit", item.Unit);
            cmd.Parameters.AddWithValue("@QtyPerUnit", item.QuanlityPerUnit);
            cmd.Parameters.AddWithValue("@Category", item.Category);
            cmd.Parameters.AddWithValue("@Description", item.Description);

            newItemId = (int)cmd.ExecuteScalar();

            var cinemaCmd = new SqlCommand("SELECT CinemaID FROM Cinemas", conn);
            var reader = cinemaCmd.ExecuteReader();
            var cinemaIds = new List<int>();

            while (reader.Read())
                cinemaIds.Add((int)reader["CinemaID"]);

            reader.Close();

            foreach (var cinemaId in cinemaIds)
            {
                var insertStockCmd = new SqlCommand(@"
                INSERT INTO Cinemas_ItemsStock (CinemaID, ItemID, Quantity, Note)
                VALUES (@CinemaID, @ItemID, @Quantity, @Note)", conn);

                insertStockCmd.Parameters.AddWithValue("@CinemaID", cinemaId);
                insertStockCmd.Parameters.AddWithValue("@ItemID", newItemId);
                insertStockCmd.Parameters.AddWithValue("@Quantity", 0);
                insertStockCmd.Parameters.AddWithValue("@Note", $"Khởi tạo bởi Admin. SL/SP: {item.QuanlityPerUnit}");

                insertStockCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
            
        }

        public IActionResult EditItem (ItemModel item)
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
            cmd.Parameters.AddWithValue("@Name", item.ItemName);
            cmd.Parameters.AddWithValue("@Unit", item.Unit);
            cmd.Parameters.AddWithValue("@QtyPerUnit", item.QuanlityPerUnit);
            cmd.Parameters.AddWithValue("@Category", item.Category);
            cmd.Parameters.AddWithValue("@Description", item.Description);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }
            
        public IActionResult Delete (int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var delStockCmd = new SqlCommand ("Delete from Cinemas_ItemsStock where ItemID = @id", conn)
        }

    }
}
