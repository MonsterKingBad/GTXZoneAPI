using GTXZone.Data;
using GTXZone.Models;
using GTXZone.Services;
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
        private readonly SupabaseStorageService _storageService;

        public GamesController(
            AppDbContext context,
            SupabaseStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

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

            var games = await query.OrderByDescending(g => g.Id).ToListAsync();
            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
                return NotFound(new { message = "Game not found" });

            return Ok(game);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [RequestSizeLimit(1024L * 1024L * 1024L)]
        public async Task<IActionResult> Create([FromForm] GameCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Title is required" });

            string? fileUrl = null;

            if (dto.File != null && dto.File.Length > 0)
            {
                fileUrl = await _storageService.UploadFileAsync(dto.File);
            }

            var game = new Game
            {
                Title = dto.Title.Trim(),
                Description = dto.Description,
                Genre = dto.Genre,
                ImageUrl = dto.ImageUrl,
                FilePath = fileUrl
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return Ok(game);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [RequestSizeLimit(1024L * 1024L * 1024L)]
        public async Task<IActionResult> Update(int id, [FromForm] GameUpdateDto dto)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
                return NotFound(new { message = "Game not found" });

            game.Title = dto.Title?.Trim() ?? game.Title;
            game.Description = dto.Description;
            game.Genre = dto.Genre;
            game.ImageUrl = dto.ImageUrl;

            if (dto.File != null && dto.File.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(game.FilePath))
                {
                    await _storageService.DeleteFileAsync(game.FilePath);
                }

                game.FilePath = await _storageService.UploadFileAsync(dto.File);
            }

            await _context.SaveChangesAsync();
            return Ok(game);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
                return NotFound(new { message = "Game not found" });

            if (!string.IsNullOrWhiteSpace(game.FilePath))
            {
                await _storageService.DeleteFileAsync(game.FilePath);
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}