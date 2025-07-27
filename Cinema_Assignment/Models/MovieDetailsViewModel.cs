using System;
using System.Collections.Generic;
using Cinema_Assignment.Models;

namespace Cinema_Assignment.Models
{
    public class MovieDetailsViewModel
    {
        public MovieModel Movie { get; set; }
        public List<ShowTimeModel> Showtimes { get; set; }
        public string SelectedDate { get; set; }
    }
}
