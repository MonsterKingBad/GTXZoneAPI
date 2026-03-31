using GTXZone.Models;
using Microsoft.EntityFrameworkCore;

namespace GTXZone.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor (REQUIRED)
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
    }
}