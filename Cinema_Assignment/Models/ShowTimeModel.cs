namespace Cinema_Assignment.Models
{
    public class ShowTimeModel
    {
        public int ShowTimeID { get; set; }
        public int RoomID { get; set; }
        public int MovieID { get; set; }
        public int Duration { get; set; }
        public string MovieName { get; set; }
        public string RoomName { get; set; }
        public string CinemaName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
