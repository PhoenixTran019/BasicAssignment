namespace Cinema_Assignment.Models
{
    public class ItemStockViewModel
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public int QuanlityPerUnit { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
    }
}
