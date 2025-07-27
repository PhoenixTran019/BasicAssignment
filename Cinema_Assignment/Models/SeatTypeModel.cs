namespace Cinema_Assignment.Models
{
    public class SeatTypeModel
    {
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        public string Color { get; set; } // dùng trong View (VD: #ffe08a)
    }
}
