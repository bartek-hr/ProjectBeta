using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBeta.Model;

namespace ProjectBeta.Data;

public class AppDbContext : DbContext
{
    // Add Models below
    public DbSet<User> Users { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Cinema> Cinemas {get; set; }
    public DbSet<Auditorium> Auditoriums {get; set; }

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

        modelBuilder.Entity<Auditorium>()
            .HasOne(a => a.Cinema)
            .WithMany(c => c.Auditoriums)
            .HasForeignKey(a => a.CinemaId);

        modelBuilder.Entity<Cinema>().HasData(
            new Cinema { Id = 1, Name = "Darcy", City = "Rotterdam" }
        );

        modelBuilder.Entity<Auditorium>().HasData(
            new Auditorium { Id = 1, Name = "Auditorium 1", CinemaId = 1, Capacity = 150 },
            new Auditorium { Id = 2, Name = "Auditorium 2", CinemaId = 1, Capacity = 300 },
            new Auditorium { Id = 3, Name = "Auditorium 3", CinemaId = 1, Capacity = 500 }
        );
    }
}
