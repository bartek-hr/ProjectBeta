using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBeta.Model;

namespace ProjectBeta.Data;

public class AppDbContext : DbContext
{
    // Add Models below
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "password",
                Role = "Admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                DateOfBirth = new DateOnly(1990, 1, 1),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Id = 2,
                Username = "user1",
                PasswordHash = "password",
                Role = "User",
                Email = "user1@example.com",
                FirstName = "User",
                LastName = "One",
                DateOfBirth = new DateOnly(1995, 5, 15),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
        );
    }
}
