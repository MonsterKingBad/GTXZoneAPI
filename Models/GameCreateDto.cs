namespace GTXZone.Models
{
    public class GameCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
    }
}