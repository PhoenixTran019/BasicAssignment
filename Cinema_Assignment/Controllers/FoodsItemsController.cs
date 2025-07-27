using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using Cinema_Assignment.Models;


namespace Cinema_Assignment.Controllers
{
    public class FoodsItemsController : Controller
    {
        private readonly IConfiguration _configuration;

        private string ConnectionString => _configuration.GetConnectionString("DefaultConnection");

        public FoodsItemsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        bool isAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        private string GetFoodName(int foodId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT Name FROM Foods WHERE FoodID = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", foodId);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private List<FoodItemDetailModel> GetAllItems()
        {
            var items = new List<FoodItemDetailModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT ItemID, ItemName FROM Items";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new FoodItemDetailModel
                    {
                        ItemID = (int)reader["ItemID"],
                        ItemName = reader["ItemName"].ToString()
                    });
                }
            }
            return items;
        }

        //Get: FoodsItems
        public IActionResult IndexAllFormula()
        {
            if(!isAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }

            var list = new List<FoodFormulaViewModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"
                SELECT f.FoodID, f.Name AS FoodName,
                       COUNT(fi.ItemID) AS ItemCount
                FROM Foods f
                LEFT JOIN Foods_Items fi ON f.FoodID = fi.FoodID
                GROUP BY f.FoodID, f.Name";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new FoodFormulaViewModel
                    {
                        FoodID = (int)reader["FoodID"],
                        FoodName = reader["FoodName"].ToString()
                    });
                }
            }
            return View(list);
        }

        // GET: CreateFormula
        public IActionResult CreateFormula(int foodId)
        {
            var model = new FoodFormulaViewModel { FoodID = foodId, Items = new List<FoodItemDetailModel>() };
            ViewBag.FoodName = GetFoodName(foodId);
            ViewBag.AllItems = GetAllItems();
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateFormula(FoodFormulaViewModel model)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                foreach (var item in model.Items)
                {
                    string insert = @"INSERT INTO Foods_Items (FoodID, ItemID, QuantityPerFood)
                                  VALUES (@FoodID, @ItemID, @QuantityPerFood)";
                    SqlCommand cmd = new SqlCommand(insert, conn);
                    cmd.Parameters.AddWithValue("@FoodID", model.FoodID);
                    cmd.Parameters.AddWithValue("@ItemID", item.ItemID);
                    cmd.Parameters.AddWithValue("@QuantityPerFood", item.QuantityPerFood);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("IndexFormula");
        }

        // GET: EditFormula
        public IActionResult EditFormula(int foodId)
        {
            var model = new FoodFormulaViewModel { FoodID = foodId, Items = new List<FoodItemDetailModel>() };
            ViewBag.FoodName = GetFoodName(foodId);
            ViewBag.AllItems = GetAllItems();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"SELECT fi.ItemID, i.ItemName, fi.QuantityPerFood
                             FROM Foods_Items fi
                             JOIN Items i ON fi.ItemID = i.ItemID
                             WHERE fi.FoodID = @FoodID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FoodID", foodId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    model.Items.Add(new FoodItemDetailModel
                    {
                        ItemID = (int)reader["ItemID"],
                        ItemName = reader["ItemName"].ToString(),
                        QuantityPerFood = (int)reader["QuantityPerFood"]
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult EditFormula(FoodFormulaViewModel model)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                // Xóa công thức cũ
                string delete = "DELETE FROM Foods_Items WHERE FoodID = @FoodID";
                SqlCommand deleteCmd = new SqlCommand(delete, conn);
                deleteCmd.Parameters.AddWithValue("@FoodID", model.FoodID);
                deleteCmd.ExecuteNonQuery();

                // Chèn công thức mới
                foreach (var item in model.Items)
                {
                    string insert = @"INSERT INTO Foods_Items (FoodID, ItemID, QuantityPerFood)
                                  VALUES (@FoodID, @ItemID, @QuantityPerFood)";
                    SqlCommand cmd = new SqlCommand(insert, conn);
                    cmd.Parameters.AddWithValue("@FoodID", model.FoodID);
                    cmd.Parameters.AddWithValue("@ItemID", item.ItemID);
                    cmd.Parameters.AddWithValue("@QuantityPerFood", item.QuantityPerFood);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("IndexFormula");
        }
        public IActionResult DeleteFormula(int foodId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string delete = "DELETE FROM Foods_Items WHERE FoodID = @FoodID";
                SqlCommand cmd = new SqlCommand(delete, conn);
                cmd.Parameters.AddWithValue("@FoodID", foodId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("IndexFormula");
        }


    }
}
