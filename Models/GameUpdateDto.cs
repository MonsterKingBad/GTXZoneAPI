using Microsoft.AspNetCore.Http;

namespace GTXZone.Models
{
    public class GameUpdateDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Genre { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public IFormFile? File { get; set; }
        public bool RemoveFile { get; set; } = false;
    }
}