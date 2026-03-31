using GTXZone.Data;
using GTXZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GTXZone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/games
        // GET: api/games?search=gta
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Games.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(g =>
                    g.Title.Contains(search) ||
                    g.Genre.Contains(search) ||
                    g.Description.Contains(search));
            }

            var games = await query.ToListAsync();
            return Ok(games);
        }

        // GET: api/games/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
                return NotFound(new { message = "Game not found" });

            return Ok(game);
        }

        // POST: api/games
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] GameCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required");

            string? filePath = null;

            if (dto.File != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                filePath = "/Uploads/" + fileName;
            }

            var game = new Game
            {
                Title = dto.Title,
                Description = dto.Description,
                Genre = dto.Genre,
                ImageUrl = dto.ImageUrl,
                FilePath = filePath
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return Ok(game);
        }

        // PUT: api/games/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] GameUpdateDto dto)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return NotFound("Game not found");

            game.Title = dto.Title;
            game.Description = dto.Description;
            game.Genre = dto.Genre;
            game.ImageUrl = dto.ImageUrl;

            // remove file
            if (dto.RemoveFile && game.FilePath != null)
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), game.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                game.FilePath = null;
            }

            // upload new file
            if (dto.File != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                game.FilePath = "/Uploads/" + fileName;
            }

            await _context.SaveChangesAsync();
            return Ok(game);
        }

        // DELETE: api/games/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
                return NotFound(new { message = "Game not found" });

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}