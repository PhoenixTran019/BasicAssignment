using Cinema_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Cinema_Assignment.Controllers
{
    public class ShowTimesController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly string connectionString;

        public ShowTimesController(IConfiguration configuration)
        {
            configuration = configuration;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll") == 1;
        }

        public IActionResult IndexAllShowtimes()
        {
            List<ShowTimeModel> list = new List<ShowTimeModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
        SELECT s.ShowTimeID, s.StartTime, s.EndTime, m.Title AS MovieName, r.RoomName AS RoomName, c.CinemaName AS CinemaName
        FROM ShowTimes s
        JOIN Movies m ON s.MovieID = m.MovieID
        JOIN Rooms r ON s.RoomID = r.RoomID
        JOIN Cinemas c ON r.CinemaID = c.CinemaID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new ShowTimeModel
                    {
                        ShowTimeID = (int)reader["ShowTimeID"],
                        MovieName = reader["MovieName"].ToString(),
                        RoomName = reader["RoomName"].ToString(),
                        CinemaName = reader["CinemaName"].ToString(),
                        StartTime = (DateTime)reader["StartTime"],
                        EndTime = (DateTime)reader["EndTime"]
                    });
                }
            }

            return View("IndexAllShowTimes", list);
        }


        public IActionResult Index(int roomId)
        {
            List<ShowTimeModel> showTimes = new List<ShowTimeModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
            SELECT s.ShowTimeID, m.Title AS MovieName, s.StartTime, s.EndTime
            FROM ShowTimes s
            JOIN Movies m ON s.MovieID = m.MovieID
            WHERE s.RoomID = @RoomID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RoomID", roomId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    showTimes.Add(new ShowTimeModel
                    {
                        ShowTimeID = (int)reader["ShowTimeID"],
                        MovieName = reader["MovieName"].ToString(),
                        StartTime = (DateTime)reader["StartTime"],
                        EndTime = (DateTime)reader["EndTime"]
                    });
                }
            }

            ViewBag.RoomID = roomId;

            return View(showTimes);
        }

        // Chọn Cinema
        public IActionResult SelectCinemas()
        {
            List<CinemaModel> cinemas = new List<CinemaModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Cinemas";
                SqlCommand cmd = new SqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cinemas.Add(new CinemaModel
                    {
                        CinemaID = (int)reader["CinemaID"],
                        CinemaName = reader["CinemaName"].ToString()
                    });
                }
            }

            return View(cinemas);
        }

        public IActionResult SelectRooms(int cinemaId)
        {
            ViewBag.CinemaID = cinemaId;
            List<RoomsModel> rooms = new List<RoomsModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Rooms WHERE CinemaID = @CinemaID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CinemaID", cinemaId);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rooms.Add(new RoomsModel
                    {
                        RoomID = (int)reader["RoomID"],
                        RoomName = reader["RoomName"].ToString()
                    });
                }
            }

            return View(rooms);
        }

        public int GetMovieDuration(int movieId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Duration FROM Movies WHERE MovieID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", movieId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public string GetMovieName(int movieId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Title FROM Movies WHERE MovieID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", movieId);
                return cmd.ExecuteScalar().ToString();
            }
        }

        public int GenerateNextShowtimeID()
        {
            int nextID = 100000;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(MAX(ShowtimeID), 99999) + 1 FROM Showtimes";
                SqlCommand cmd = new SqlCommand(sql, conn);
                nextID = (int)cmd.ExecuteScalar();
            }

            return nextID;
        }

        public List<MovieModel> GetAllMovies()
        {
            List<MovieModel> movies = new List<MovieModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT MovieID, Title, Duration FROM Movies";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    movies.Add(new MovieModel
                    {
                        MovieID = (int)reader["MovieID"],
                        Title = reader["Title"].ToString(),
                        Duration = (int)reader["Duration"]
                    });
                }
            }

            return movies;
        }

        // GET: Create
        public IActionResult CreateShowtimes(int roomId)
        {
            var showTime = new ShowTimeModel
            {
                RoomID = roomId,
                ShowTimeID = GenerateNextShowtimeID()
            };

            ViewBag.Movies = GetAllMovies(); // Truyền danh sách phim

            return View(showTime);
        }



        // POST: Create
        [HttpPost]
        public IActionResult CreateShowtimes(ShowTimeModel showTime)
        {
            int duration = GetMovieDuration(showTime.MovieID);
            showTime.EndTime = showTime.StartTime.AddMinutes(duration);

            // Ràng buộc 1: Ngày tạo phải cách 3 ngày
            if ((showTime.StartTime - DateTime.Now).TotalDays < 3)
            {
                ModelState.AddModelError("", "Xuất chiếu phải được tạo cách ít nhất 3 ngày so với ngày hiện tại.");
                return View(showTime); // trả về view với lỗi
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Ràng buộc 2: Không trùng giờ chiếu trong cùng phòng (cách nhau < 45 phút)
                string overlapCheck = @"
            SELECT COUNT(*) FROM ShowTimes 
            WHERE RoomID = @RoomID AND (
                (StartTime <= @NewStartTime AND EndTime > @NewStartTime) OR
                (StartTime < @NewEndTime AND EndTime >= @NewEndTime) OR
                (StartTime >= @NewStartTime AND EndTime <= @NewEndTime)
            )";

                SqlCommand checkCmd = new SqlCommand(overlapCheck, conn);
                checkCmd.Parameters.AddWithValue("@RoomID", showTime.RoomID);
                checkCmd.Parameters.AddWithValue("@NewStartTime", showTime.StartTime.AddMinutes(-45)); // cho phép cách ít nhất 45 phút
                checkCmd.Parameters.AddWithValue("@NewEndTime", showTime.EndTime.AddMinutes(45));     // cũng kiểm tra kết thúc không quá sát

                int conflictCount = (int)checkCmd.ExecuteScalar();
                if (conflictCount > 0)
                {
                    ModelState.AddModelError("", "Xuất chiếu này quá gần với một suất chiếu khác trong cùng phòng (cách nhau ít nhất 45 phút).");
                    return View(showTime);
                }

                // Nếu mọi điều kiện đều OK thì INSERT
                string sql = "INSERT INTO ShowTimes (RoomID, MovieID, StartTime, EndTime) " +
                             "VALUES (@RoomID, @MovieID, @StartTime, @EndTime)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RoomID", showTime.RoomID);
                cmd.Parameters.AddWithValue("@MovieID", showTime.MovieID);
                cmd.Parameters.AddWithValue("@StartTime", showTime.StartTime);
                cmd.Parameters.AddWithValue("@EndTime", showTime.EndTime);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("IndexAllShowTime");
        } 

        // GET: Edit
        public IActionResult EditShowtimes(int id)
        {
            ShowTimeModel showTime = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT s.*, m.Title, m.Duration FROM ShowTimes s JOIN Movies m ON s.MovieID = m.MovieID WHERE s.ShowTimeID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    showTime = new ShowTimeModel
                    {
                        ShowTimeID = (int)reader["ShowTimeID"],
                        RoomID = (int)reader["RoomID"],
                        MovieID = (int)reader["MovieID"],
                        MovieName = reader["Title"].ToString(),
                        Duration = (int)reader["Duration"],
                        StartTime = (DateTime)reader["StartTime"],
                        EndTime = (DateTime)reader["EndTime"]
                    };
                }
            }

            if (showTime == null) return NotFound();

            return View(showTime);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult EditShowTimes(ShowTimeModel showTime)
        {
            int duration = showTime.Duration; // Lấy từ model (không đổi)

            showTime.EndTime = showTime.StartTime.AddMinutes(duration);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE ShowTimes SET StartTime=@StartTime, EndTime=@EndTime WHERE ShowTimeID=@ShowTimeID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@StartTime", showTime.StartTime);
                cmd.Parameters.AddWithValue("@EndTime", showTime.EndTime);
                cmd.Parameters.AddWithValue("@ShowTimeID", showTime.ShowTimeID);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index", new { roomId = showTime.RoomID });
        }

        // GET: Delete
        public IActionResult DeleteShowtimes(int id)
        {
            ShowTimeModel showTime = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT s.ShowTimeID, s.StartTime, s.EndTime, m.Title AS MovieName FROM ShowTimes s JOIN Movies m ON s.MovieID = m.MovieID WHERE s.ShowTimeID = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    showTime = new ShowTimeModel
                    {
                        ShowTimeID = (int)reader["ShowTimeID"],
                        MovieName = reader["MovieName"].ToString(),
                        StartTime = (DateTime)reader["StartTime"],
                        EndTime = (DateTime)reader["EndTime"]
                    };
                }
            }

            if (showTime == null) return NotFound();

            return View(showTime);
        }


        // POST: Delete
        [HttpPost]
        public IActionResult DeleteShowtime(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string getSql = "SELECT StartTime FROM ShowTimes WHERE ShowtimeID = @id";
                SqlCommand getCmd = new SqlCommand(getSql, conn);
                getCmd.Parameters.AddWithValue("@id", id);
                DateTime startTime = (DateTime)getCmd.ExecuteScalar();

                if ((startTime - DateTime.Now).TotalDays < 3)
                {
                    TempData["Error"] = "Không thể xóa suất chiếu sẽ diễn ra trong vòng 3 ngày.";
                    return RedirectToAction("IndexAllShowTime");
                }

                string deleteSql = "DELETE FROM ShowTimes WHERE ShowtimeID = @id";
                SqlCommand delCmd = new SqlCommand(deleteSql, conn);
                delCmd.Parameters.AddWithValue("@id", id);
                delCmd.ExecuteNonQuery();
            }

            return RedirectToAction("IndexAllShowTime");
        }
    }
}
