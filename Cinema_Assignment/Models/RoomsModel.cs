namespace Cinema_Assignment.Models
{
    public class RoomsModel
    {
        public int RoomID { get; set; }
        public string? RoomName { get; set; }
        public string? RoomType { get; set; }
        public int CinemaID { get; set; }
        public int TotalSeat { get; set; }
    }
}
