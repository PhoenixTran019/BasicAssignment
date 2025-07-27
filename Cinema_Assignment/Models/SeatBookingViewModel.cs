using System;
using System.Collections.Generic;

namespace Cinema_Assignment.Models
{
    public class SeatBookingViewModel
    {
        public int ShowtimeId { get; set; }
        public string MovieTitle { get; set; }
        public DateTime StartTime { get; set; }

        public List<TempSeatModel> Seats { get; set; } = new();
        public HashSet<string> LockedSeatIds { get; set; } = new();
        public Dictionary<int, SeatTypeModel> SeatTypes { get; set; } = new();
    }
}
