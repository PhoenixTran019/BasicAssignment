namespace Cinema_Assignment.Models
{
   
        public class SeatLayoutDisplayModel
        {
            public char StartRow { get; set; }
            public char EndRow { get; set; }
            public string Decription { get; set; }
            public char RowChar { get; set; }
            public int ColumnNum { get; set; }
            public bool IsLock { get; set; }
            public int StartCol { get; set; }
            public int EndCol { get; set; }
            public string TypeName { get; set; }
            public decimal Price { get; set; }
        }

    
}
