namespace Cinema_Assignment.Models
{
   
        public class SeatLayoutDisplayModel
        {
            public char StartRow { get; set; }
            public char EndRow { get; set; }
            public int StartCol { get; set; }
            public int EndCol { get; set; }
            public string TypeName { get; set; }
            public decimal Price { get; set; }
        }

    
}
