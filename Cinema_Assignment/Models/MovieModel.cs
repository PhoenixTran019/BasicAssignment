namespace Cinema_Assignment.Models
{
    public class MovieModel
    {
        public int MovieID { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Genre { get; set; }
        public string Image { get; set; }
        public int AgeRequest { get; set; }
        public DateTime ReleaseDate { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}
