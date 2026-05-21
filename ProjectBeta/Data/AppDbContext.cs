using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
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
    public DbSet<Auditorium> Auditoriums {get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieSchedule> MovieSchedules { get; set; }
    public DbSet<Snack> Snacks { get; set; }
    public DbSet<BookingSnack> BookingSnacks { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<BookingDiscount> BookingDiscounts { get; set; }
    public DbSet<SeatPrice> SeatPrices { get; set; }

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
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
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
            entity.Property(schedule => schedule.AuditoriumId).IsRequired();
            entity.HasIndex(schedule => schedule.ScheduleDate);
            entity.HasIndex(schedule => new { schedule.ScheduleDate, schedule.AuditoriumId, schedule.StartTime }).IsUnique();
            entity.HasOne(schedule => schedule.Movie)
                .WithMany()
                .HasForeignKey(schedule => schedule.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(schedule => schedule.Auditorium)
                .WithMany()
                .HasForeignKey(schedule => schedule.AuditoriumId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "password",
                Role = "SuperAdmin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                DateOfBirth = new DateOnly(1990, 1, 1),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                HasSubscription = false,
                SubscriptionSeatType = null
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
                IsActive = true,
                HasSubscription = false,
                SubscriptionSeatType = null
            }
        );

        modelBuilder.Entity<Auditorium>()
            .HasOne(a => a.Location)
            .WithMany(l => l.Auditoriums)
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Location>().HasData(
            new Location { Id = 1, Name = "Main Location", City = "Rotterdam", Address = "Main St 1" }
        );

        modelBuilder.Entity<Auditorium>().HasData(
            new Auditorium { Id = 1, Name = "Auditorium 1", LocationId = 1, Capacity = 150 },
            new Auditorium { Id = 2, Name = "Auditorium 2", LocationId = 1, Capacity = 300 },
            new Auditorium { Id = 3, Name = "Auditorium 3", LocationId = 1, Capacity = 500 }
        );



        modelBuilder.Entity<BookingSnack>(entity =>
        {
            entity.HasOne(bs => bs.Snack)
                .WithMany()
                .HasForeignKey(bs => bs.SnackId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(bs => bs.Booking)
                .WithMany()
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingDiscount>(entity =>
        {
            // Restrict so soft-deleting a Discount doesn't wipe booking history.
            entity.HasOne(bd => bd.Discount)
                .WithMany()
                .HasForeignKey(bd => bd.DiscountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // Seeds default data if the table is empty. Safe to call on every startup
    public void Seed()
    {
        if (!Discounts.Any())
        {
            Discounts.AddRange(
                new Discount
                {
                    Name = "Child Discount",
                    Percentage = 50m,
                    MaxAge = 12,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow
                },
                new Discount
                {
                    Name = "Senior Discount",
                    Percentage = 20m,
                    MinAge = 65,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow
                },
                new Discount
                {
                    Name = "Group Discount",
                    Percentage = 20m,
                    MinGroupSize = 6,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow
                }
            );
            SaveChanges();
        }

        if (!SeatPrices.Any())
        {
            SeatPrices.AddRange(
                new SeatPrice { Id = 1, Name = "Standard", Price = 15.00m },
                new SeatPrice { Id = 2, Name = "VIP",      Price = 17.50m },
                new SeatPrice { Id = 3, Name = "King",     Price = 20.00m }
            );
            SaveChanges();
        }

        var staleAuditoriums = Auditoriums.Where(a =>
            (a.Name.StartsWith("Small")  && a.Capacity != 150) ||
            (a.Name.StartsWith("Medium") && a.Capacity != 300) ||
            (a.Name.StartsWith("Large")  && a.Capacity != 500)
        ).ToList();

        if (staleAuditoriums.Count > 0)
        {
            foreach (var a in staleAuditoriums)
            {
                if (a.Name.StartsWith("Small"))  a.Capacity = 150;
                else if (a.Name.StartsWith("Medium")) a.Capacity = 300;
                else if (a.Name.StartsWith("Large"))  a.Capacity = 500;
            }
            SaveChanges();
        }
    }
}
