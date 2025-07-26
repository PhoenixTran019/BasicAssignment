using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Cinema_Assignment.Models;


namespace Cinema_Assignment.Controllers
{
    public class FoodsController : Controller
    {
        private readonly string connectionString;
        private readonly string _imagePath;
        private readonly IWebHostEnvironment _env;

        public int GenerateNextFoodID()
        {
            int nextID = 1;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(MAX(FoodID), 0) + 1 FROM Foods";
                SqlCommand cmd = new SqlCommand(sql, conn);
                nextID = (int)cmd.ExecuteScalar();
            }

            return nextID;
        }

        public FoodsController(IConfiguration config, IWebHostEnvironment env)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
            _env = env;
            _imagePath = Path.Combine(env.WebRootPath, "uploads", "foods");
            if (!Directory.Exists(_imagePath))
                Directory.CreateDirectory(_imagePath);
        }

        bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        //Get: /Food
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var foods = new List<FoodModel>();

            using var conn = new SqlConnection(connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Foods", conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                foods.Add(new FoodModel
                {
                    FoodID = (int)reader["FoodID"],
                    Name = reader["Name"].ToString(),
                    Price = (decimal)reader["Price"],
                    Image = reader["Image"].ToString(),
                    Decription = reader["Decription"].ToString()
                });
            }


            return View(foods);
        }

        //Get: /Food/CreateFoods
        public IActionResult CreateFoods()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            var food = new FoodModel
            {
                FoodID = GenerateNextFoodID(),
            };
            return View(food);
        }

        //Post: /Food/CreateFoods
        [HttpPost]
        public IActionResult CreateFoods (FoodModel model)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            if (model.ImageFile != null)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string fullPath = Path.Combine(_imagePath, fileName);
                using var stream = new FileStream(fullPath, FileMode.Create);
                model.ImageFile.CopyTo(stream);
                model.Image = "/uploads/foods/" + fileName;
            }

            using var conn = new SqlConnection(connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
            INSERT INTO Foods (FoodID, Name, Price, Image, Decription)
            VALUES (@id, @name, @price, @img, @desc)", conn);
            cmd.Parameters.AddWithValue("@id", model.FoodID);
            cmd.Parameters.AddWithValue("@name", model.Name ?? "");
            cmd.Parameters.AddWithValue("@price", model.Price);
            cmd.Parameters.AddWithValue("@img", model.Image ?? "");
            cmd.Parameters.AddWithValue("@desc", model.Decription ?? "");
            cmd.ExecuteNonQuery();

            // Gán Food cho tất cả các rạp hiện có
            var cmdCinemas = new SqlCommand("SELECT CinemaID FROM Cinemas", conn);
            var reader = cmdCinemas.ExecuteReader();
            var cinemas = new List<int>();
            while (reader.Read()) cinemas.Add((int)reader["CinemaID"]);
            reader.Close();

            foreach (var cinemaID in cinemas)
            {
                var insertCmd = new SqlCommand(@"
                INSERT INTO Foods_Cinemas (CinemaID, FoodID, Status) VALUES (@c, @f, 1)", conn);
                insertCmd.Parameters.AddWithValue("@c", cinemaID);
                insertCmd.Parameters.AddWithValue("@f", model.FoodID);
                insertCmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        //Get: /Food/EditFoods
        public IActionResult EditFoods (int id)
        {
            FoodModel model = null;
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("Select * From Foods Where FoodID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    model = new FoodModel
                    {
                        FoodID = (int)reader["FoodID"],
                        Name = reader["Name"].ToString(),
                        Price = (decimal)reader["Price"],
                        Image = reader["Image"].ToString(),
                        Decription = reader["Decription"].ToString()
                    };
                }
            }
            return model == null ? NotFound() : View(model);
        }

        //Post: /Food/EditFoods
        [HttpPost]
        public IActionResult Edit(FoodModel model)
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            string imagePath = model.Image;

            // Nếu có ảnh mới → xóa ảnh cũ
            if (model.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(model.Image))
                {
                    string oldPath = Path.Combine(_env.WebRootPath, model.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string folder = Path.Combine(_env.WebRootPath, "uploads", "foods");
                Directory.CreateDirectory(folder);
                string fullPath = Path.Combine(folder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                model.ImageFile.CopyTo(stream);
                imagePath = "/uploads/foods/" + fileName;
            }

            var cmd = new SqlCommand(@"
            UPDATE Foods
            SET Name = @name, Price = @price, Image = @image, Decription = @desc
            WHERE FoodID = @id", conn);

            cmd.Parameters.AddWithValue("@name", model.Name);
            cmd.Parameters.AddWithValue("@price", model.Price);
            cmd.Parameters.AddWithValue("@image", imagePath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@desc", model.Decription ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", model.FoodID);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        //Get: /Food/DeleteFoods
        public IActionResult DeleteFoods (int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            using var conn = new SqlConnection(connectionString);
            conn.Open();

            var cmd = new SqlCommand("Delete From Foods Where FoodID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();

            if(!reader.Read()) return NotFound();

            var model = new FoodModel
            {
                FoodID = (int)reader["FoodID"],
                Name = reader["Name"].ToString(),
                Price = (decimal)reader["Price"],
                Image = reader["Image"].ToString(),
                Decription = reader["Decription"].ToString()
            };
            return View(model);
        }
    }
}
