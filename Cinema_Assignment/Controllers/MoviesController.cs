using Microsoft.AspNetCore.Mvc;
using Cinema_Assignment.Models;
using System.Data.SqlClient;

namespace Cinema_Assignment.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;

        

        public MoviesController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Lấy ID mới
        public int GenerateNextMovieID()
        {
            int nextID = 100000;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(MAX(MovieID), 99999) + 1 FROM Movies";
                SqlCommand cmd = new SqlCommand(sql, conn);
                nextID = (int)cmd.ExecuteScalar();
            }

            return nextID;
        }

        // Lấy danh sách thể loại
        public List<string> GetGenres()
        {
            return new List<string> { "Hành động", "Kinh dị", "Tình cảm", "Hài", "Khoa học viễn tưởng", "Hoạt hình", "Khác" };
        }
        bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        // Danh sách phim
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            List<MovieModel> movies = new List<MovieModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Movies";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    movies.Add(new MovieModel
                    {
                        MovieID = (int)reader["MovieID"],
                        Title = reader["Title"].ToString(),
                        Duration = (int)reader["Duration"],
                        Genre = reader["Genre"].ToString(),
                        AgeRequest = (int)reader["AgeRequest"],
                        Image = reader["Image"].ToString(),
                        ReleaseDate = (DateTime)reader["ReleaseDate"]
                    });
                }
            }

            return View(movies);
        }

        // GET Create
        public IActionResult CreateMovies()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            var movie = new MovieModel
            {
                MovieID = GenerateNextMovieID(),
                ReleaseDate = DateTime.Now.Date
            };

            ViewBag.Genres = GetGenres();
            return View(movie);
        }

        // POST Create
        [HttpPost]
        public IActionResult CreateMovies(MovieModel movie)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            string genre = movie.Genre;
            if (genre == "Khác")
            {
                genre = Request.Form["OtherGenre"];
            }

            // Xử lý ảnh
            if (movie.ImageFile != null)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/movies");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(movie.ImageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    movie.ImageFile.CopyTo(stream);
                }

                movie.Image = "/uploads/movies/" + fileName;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Movies (MovieID, Title, Duration, Genre, Image, ReleaseDate, AgeRequest) " +
                             "VALUES (@MovieID, @Title, @Duration, @Genre, @Image, @ReleaseDate, @AgeRequest)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MovieID", movie.MovieID);
                cmd.Parameters.AddWithValue("@Title", movie.Title);
                cmd.Parameters.AddWithValue("@Duration", movie.Duration);
                cmd.Parameters.AddWithValue("@Genre", genre);
                cmd.Parameters.AddWithValue("@Image", (object)movie.Image ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ReleaseDate", movie.ReleaseDate);
                cmd.Parameters.AddWithValue("@AgeRequest", movie.AgeRequest);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // GET Edit
        public IActionResult EditMovies(int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            MovieModel movie = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Movies WHERE MovieID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    movie = new MovieModel
                    {
                        MovieID = (int)reader["MovieID"],
                        Title = reader["Title"].ToString(),
                        Duration = (int)reader["Duration"],
                        Genre = reader["Genre"].ToString(),
                        Image = reader["Image"].ToString(),
                        ReleaseDate = (DateTime)reader["ReleaseDate"],
                        AgeRequest = (int)reader["AgeRequest"]
                    };
                }
            }

            if (movie == null) return NotFound();

            ViewBag.Genres = GetGenres();
            return View(movie);
        }

        // POST Edit
        [HttpPost]
        public IActionResult EditMovies(MovieModel movie)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            string genre = movie.Genre;
            if (genre == "Khác")
            {
                genre = Request.Form["OtherGenre"];
            }

            string oldImage = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Image FROM Movies WHERE MovieID=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", movie.MovieID);
                oldImage = cmd.ExecuteScalar()?.ToString();
            }

            if (movie.ImageFile != null)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/movies");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(movie.ImageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    movie.ImageFile.CopyTo(stream);
                }

                movie.Image = "/uploads/movies/" + fileName;

                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(oldImage))
                {
                    string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
            }
            else
            {
                movie.Image = oldImage; // Giữ ảnh cũ
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE Movies SET Title=@Title, Duration=@Duration, Genre=@Genre, AgeRequest = @AgeRequest, Image=@Image, ReleaseDate=@ReleaseDate WHERE MovieID=@MovieID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Title", movie.Title);
                cmd.Parameters.AddWithValue("@Duration", movie.Duration);
                cmd.Parameters.AddWithValue("@Genre", genre);
                cmd.Parameters.AddWithValue("@AgeRequest", movie.AgeRequest);
                cmd.Parameters.AddWithValue("@Image", (object)movie.Image ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ReleaseDate", movie.ReleaseDate);
                cmd.Parameters.AddWithValue("@MovieID", movie.MovieID);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // GET Delete
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            MovieModel movie = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Movies WHERE MovieID=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    movie = new MovieModel
                    {
                        MovieID = (int)reader["MovieID"],
                        Title = reader["Title"].ToString(),
                        Duration = (int)reader["Duration"],
                        Genre = reader["Genre"].ToString(),
                        AgeRequest = (int)reader["AgeRequest"],
                        Image = reader["Image"].ToString(),
                        ReleaseDate = (DateTime)reader["ReleaseDate"]
                    };
                }
            }

            if (movie == null) return NotFound();

            return View(movie);
        }

        // POST Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {

            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            string image = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Image FROM Movies WHERE MovieID=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                image = cmd.ExecuteScalar()?.ToString();

                sql = "DELETE FROM Movies WHERE MovieID=@id";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            if (!string.IsNullOrEmpty(image))
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.TrimStart('/'));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            return RedirectToAction("Index");
        }
    }
}
