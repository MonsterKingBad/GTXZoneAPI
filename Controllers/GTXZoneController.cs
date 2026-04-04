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

        public GamesController(AppDbContext context, SupabaseStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category)
        {
            var query = _context.Games.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(g =>
                    g.Title.Contains(search) ||
                    g.Genre.Contains(search) ||
                    g.Description.Contains(search) ||
                    (g.Category != null && g.Category.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                category = category.Trim();

                query = query.Where(g =>
                    g.Category != null &&
                    g.Category.ToLower() == category.ToLower());
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
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest(new { message = "Title is required" });

                if (string.IsNullOrWhiteSpace(dto.Category))
                    return BadRequest(new { message = "Category is required" });

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
                    Category = dto.Category.Trim(),
                    ImageUrl = dto.ImageUrl,
                    FilePath = fileUrl
                };

                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                return Ok(game);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Create failed",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [RequestSizeLimit(1024L * 1024L * 1024L)]
        public async Task<IActionResult> Update(int id, [FromForm] GameUpdateDto dto)
        {
            try
            {
                Console.WriteLine($"[UPDATE] Start update for game id={id}");

                var game = await _context.Games.FindAsync(id);

                if (game == null)
                    return NotFound(new { message = "Game not found" });

                Console.WriteLine($"[UPDATE] Game found. Current FilePath={game.FilePath}");

                if (!string.IsNullOrWhiteSpace(dto.Title))
                    game.Title = dto.Title.Trim();

                game.Description = dto.Description;
                game.Genre = dto.Genre;
                game.ImageUrl = dto.ImageUrl;

                if (!string.IsNullOrWhiteSpace(dto.Category))
                    game.Category = dto.Category.Trim();

                Console.WriteLine("[UPDATE] Basic fields updated");

                if (dto.File != null && dto.File.Length > 0)
                {
                    Console.WriteLine($"[UPDATE] New file detected: {dto.File.FileName}, size={dto.File.Length}");

                    if (!string.IsNullOrWhiteSpace(game.FilePath))
                    {
                        Console.WriteLine($"[UPDATE] Deleting old file: {game.FilePath}");
                        await _storageService.DeleteFileAsync(game.FilePath);
                        Console.WriteLine("[UPDATE] Old file delete finished");
                    }

                    Console.WriteLine("[UPDATE] Uploading new file to Supabase");
                    game.FilePath = await _storageService.UploadFileAsync(dto.File);
                    Console.WriteLine($"[UPDATE] Upload finished. New FilePath={game.FilePath}");
                }
                else
                {
                    Console.WriteLine("[UPDATE] No file uploaded");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("[UPDATE] SaveChanges finished");

                return Ok(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UPDATE ERROR] {ex}");
                return StatusCode(500, new
                {
                    message = "Update failed",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Delete failed",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}