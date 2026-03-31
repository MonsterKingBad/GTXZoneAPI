using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GTXZone.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=dpg-d75qr40gjchc73eudf70-a.oregon-postgres.render.com;Port=5432;Database=gtxzone_database;Username=gtxzone_database_user;Password=VQTZZkafYwKnGBDEGPA90y33FNROOWLi;SSL Mode=Require;Trust Server Certificate=true"
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}