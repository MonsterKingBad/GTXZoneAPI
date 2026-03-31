namespace GTXZone.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? FilePath { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Genre { get; set; } = "";
    }
}