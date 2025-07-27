using Microsoft.AspNetCore.Mvc;
using Cinema_Assignment.Models;
using System.Data.SqlClient;

namespace Cinema_Assignment.Controllers
{
    public class CustomerController : Controller
    {
        private readonly string connectionString;

        public CustomerController(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IActionResult Home()
        {
            List < MovieModel> movies = new List<MovieModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT MovieID, Title, Image, Genre, ReleaseDate, Duration FROM Movies", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    movies.Add(new MovieModel
                    {
                        MovieID = (int)reader["MovieID"],
                        Title = reader["Title"].ToString(),
                        Genre = reader["Genre"].ToString (),
                        Image = reader["Image"].ToString(),
                        ReleaseDate = Convert.ToDateTime(reader["ReleaseDate"]),
                        Duration = Convert.ToInt32(reader["Duration"])
                    });
                }
            }

            return View(movies);
        }
    }
}
