namespace Cinema_Assignment.Models
{
    public class SeatModel
    {
        public string SeatID { get; set; }
        public string SeatName { get; set; }
        public char RowChar { get; set; }
        public int ColumNum { get; set; }
        public int TypeID { get; set; }
        public bool IsLock { get; set; }
        public string Decription { get; set; }
        public decimal Price { get; set; }
    }
}
