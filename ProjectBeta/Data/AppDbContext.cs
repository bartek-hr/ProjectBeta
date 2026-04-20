using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieSchedule> MovieSchedules { get; set; }

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite(SqliteConnectionStringResolver.GetResolvedConnectionString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var genresConverter = new ValueConverter<List<string>, string>(
            genres => JsonSerializer.Serialize(genres, (JsonSerializerOptions?)null),
            genresJson => JsonSerializer.Deserialize<List<string>>(genresJson, (JsonSerializerOptions?)null) ?? new List<string>());

        var genresComparer = new ValueComparer<List<string>>(
            (left, right) => (left == null && right == null) || (left != null && right != null && left.SequenceEqual(right)),
            genres => genres.Aggregate(0, (hash, genre) => HashCode.Combine(hash, genre.GetHashCode())),
            genres => genres.ToList());

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(movie => movie.Id);
            entity.Property(movie => movie.Id).IsRequired();
            entity.Property(movie => movie.Title).IsRequired();
            entity.Property(movie => movie.Description).IsRequired();
            entity.Property(movie => movie.Genres)
                .HasConversion(genresConverter)
                .Metadata.SetValueComparer(genresComparer);
        });

        modelBuilder.Entity<MovieSchedule>(entity =>
        {
            entity.HasKey(schedule => schedule.Id);
            entity.Property(schedule => schedule.MovieId).IsRequired();
            entity.Property(schedule => schedule.ScheduleDate).IsRequired();
            entity.Property(schedule => schedule.StartTime).IsRequired();
            entity.Property(schedule => schedule.EndTime).IsRequired();
            entity.HasIndex(schedule => schedule.ScheduleDate);
            entity.HasIndex(schedule => new { schedule.ScheduleDate, schedule.StartTime }).IsUnique();
            entity.HasOne(schedule => schedule.Movie)
                .WithMany()
                .HasForeignKey(schedule => schedule.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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
