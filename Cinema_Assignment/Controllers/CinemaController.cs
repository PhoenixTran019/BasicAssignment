using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using Cinema_Assignment.Models;

namespace Cinema_Assignment.Controllers
{
    
    public class CinemaController : Controller
    {
        private readonly string _connectionString;

        public CinemaController(IConfiguration config)
        {
                        _connectionString = config.GetConnectionString("DefaultConnection");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Employee" && HttpContext.Session.GetInt32("UserRoll")==1;
        }

        public IActionResult Index()
        {
            if(!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            List<CinemaModel> cinemas = new();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Cinemas", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cinemas.Add(new CinemaModel
                {
                    CinemaID = (int)reader["CinemaID"],
                    CinemaName = reader["CinemaName"].ToString(),
                    CinemaCode = reader["CinemaCode"].ToString(),
                    Address = reader["Address"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()
                });
            }
            return View(cinemas);
        }

        public IActionResult Create()
        {
            if(!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }
        // POST: /Cinema/Create
        [HttpPost]
        public IActionResult Create(CinemaModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
            INSERT INTO Cinemas (CinemaID, CinemaName, CinemaCode, Address, PhoneNumber)
            VALUES (@id, @name, @code, @addr, @phone)", conn);

            cmd.Parameters.AddWithValue("@id", model.CinemaID);
            cmd.Parameters.AddWithValue("@name", model.CinemaName);
            cmd.Parameters.AddWithValue("@code", model.CinemaCode);
            cmd.Parameters.AddWithValue("@addr", model.Address);
            cmd.Parameters.AddWithValue("@phone", model.PhoneNumber);

            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        // GET: /Cinema/Edit/5
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            CinemaModel cinema = null;
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Cinemas WHERE CinemaID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                cinema = new CinemaModel
                {
                    CinemaID = (int)reader["CinemaID"],
                    CinemaName = reader["CinemaName"].ToString(),
                    CinemaCode = reader["CinemaCode"].ToString(),
                    Address = reader["Address"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()

                };
            }
            if (cinema == null) return NotFound();
            return View(cinema);
        }

        // POST: /Cinema/Edit/5
        [HttpPost]
        public IActionResult Edit(CinemaModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
            UPDATE Cinemas
            SET CinemaName = @name, CinemaCode = @code, Address = @addr, PhoneNumber = @phone
            WHERE CinemaID = @id", conn);

            cmd.Parameters.AddWithValue("@id", model.CinemaID);
            cmd.Parameters.AddWithValue("@name", model.CinemaName);
            cmd.Parameters.AddWithValue("@code", model.CinemaCode);
            cmd.Parameters.AddWithValue("@addr", model.Address);
            cmd.Parameters.AddWithValue("@phone", model.PhoneNumber);

            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        
        // GET: /Cinema/Delete/5
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            CinemaModel cinema = null;
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Cinemas WHERE CinemaID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                cinema = new CinemaModel
                {
                    CinemaID = (int)reader["CinemaID"],
                    CinemaName = reader["CinemaName"].ToString(),
                    CinemaCode = reader["CinemaCode"].ToString(),
                    Address = reader["Address"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()
                };
            }
            if (cinema == null) return NotFound();
            return View(cinema);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM Cinemas WHERE CinemaID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }
    }

}
