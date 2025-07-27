namespace Cinema_Assignment.Models
{
    public class FoodFormulaViewModel
    {
        public int FoodID { get; set; }
        public string FoodName { get; set; }
        public List<FoodItemDetailModel> Items { get; set; } = new();
    }
}
