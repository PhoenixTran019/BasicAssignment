using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Cinema_Assignment.Models;

namespace Cinema_Assignment.Controllers
{
    public class CustomerMoviesController : Controller
    {
        private readonly string connectionString;

        public CustomerMoviesController(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IActionResult Details(int id, string selectedDate = null)
        {
            MovieModel movie = null;
            List<ShowTimeModel> showtimes = new List<ShowTimeModel>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Lấy thông tin phim
                var movieCmd = new SqlCommand("SELECT * FROM Movies WHERE MovieID = @id", conn);
                movieCmd.Parameters.AddWithValue("@id", id);

                using (var reader = movieCmd.ExecuteReader())
                {
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

                            // Thêm thuộc tính nếu cần
                        };
                    }
                }

                if (movie == null) return NotFound();

                // Ngày được chọn hoặc hôm nay
                DateTime date = string.IsNullOrEmpty(selectedDate) ? DateTime.Today : DateTime.Parse(selectedDate);

                // Lấy xuất chiếu
                var showtimeCmd = new SqlCommand(@"
                    SELECT s.*, r.RoomName AS RoomName
                    FROM Showtimes s
                    INNER JOIN Rooms r ON s.RoomID = r.RoomID
                    WHERE s.MovieID = @id AND CAST(s.StartTime AS DATE) = @date
                    ORDER BY s.StartTime", conn);

                showtimeCmd.Parameters.AddWithValue("@id", id);
                showtimeCmd.Parameters.AddWithValue("@date", date);

                using (var reader = showtimeCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        showtimes.Add(new ShowTimeModel
                        {
                            ShowTimeID = (int)reader["ShowtimeID"],
                            MovieID = (int)reader["MovieID"],
                            RoomID = (int)reader["RoomID"],
                            StartTime = (DateTime)reader["StartTime"],
                            Room = new RoomsModel
                            {
                                RoomID = (int)reader["RoomID"],
                                RoomName = reader["RoomName"].ToString()
                            }
                        });
                    }
                }
            }

            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                Showtimes = showtimes,
                SelectedDate = selectedDate ?? DateTime.Today.ToString("yyyy-MM-dd")
            };

            return View(viewModel);
        }
    }
}
