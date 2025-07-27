namespace Cinema_Assignment.Models
{
    public class ToggleSeatsRequest
    {
        public List<string> SeatNames { get; set; }
        public int RoomId { get; set; }

        public char RowChar { get; set; }
        public int ColumNum { get; set; }
        public int TypeID { get; set; }
    }
}
