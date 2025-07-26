using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema_Assignment.Models
{
    public class FoodModel
    {
        public int FoodID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public string Decription { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
