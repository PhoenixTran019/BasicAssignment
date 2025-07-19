namespace Cinema_Assignment.Models
{
    public class SeatLayoutConfigModel
    {
        public int LayoutID { get; set; }
        public int RoomID { get; set; }
        public char StartRow { get; set; }
        public char EndRow { get; set; }
        public int StartCol { get; set; }
        public int EndCol { get; set; }
        public int SeatTypeID { get; set; }
    }
}
