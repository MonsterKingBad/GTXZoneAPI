using GTXZone.Data;
using GTXZone.Models;
using GTXZone.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// PostgreSQL database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=dpg-d75qr40gjchc73eudf70-a.oregon-postgres.render.com;Port=5432;Database=gtxzone_database;Username=gtxzone_database_user;Password=VQTZZkafYwKnGBDEGPA90y33FNROOWLi;SSL Mode=Require;Trust Server Certificate=true"));

// JWT service
builder.Services.AddScoped<JwtService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://gtxzone.netlify.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("SUPER_SECRET_KEY_123456789_ABCDEFG")
            )
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Create Uploads folder if it doesn't exist
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Apply migrations and seed admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin"
        });

        db.SaveChanges();
    }
}

app.UseHttpsRedirection();

// Serve default static files
app.UseStaticFiles();

// Serve files from Uploads folder
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".torrent"] = "application/x-bittorrent";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads",
    ContentTypeProvider = contentTypeProvider
});

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();