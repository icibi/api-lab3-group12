namespace lab3app.Models
{
    public class Movie
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Director { get; set; }
        public string Genre { get; set; }
        public string ReleaseTime { get; set; }
        public string UploadedBy { get; set; }
        public string MovieKey { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Rating> Ratings { get; set; }
    }

}
