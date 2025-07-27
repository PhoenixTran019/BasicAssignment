namespace Cinema_Assignment.Models
{
    public class TempSeatModel
    {
        public char Row { get; set; }
        public int Col { get; set; }
        public int TypeID { get; set; }
        public string SeatID => $"{Row}{Col}";
    }
}
