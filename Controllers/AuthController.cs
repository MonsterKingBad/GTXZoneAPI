using GTXZone.Data;
using GTXZone.Models;
using GTXZone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GTXZone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request is required" });

            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { message = "Username is required" });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Password is required" });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordCorrect)
                return Unauthorized(new { message = "Invalid username or password" });

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token = token,
                username = user.Username,
                role = user.Role
            });
        }
    }
}