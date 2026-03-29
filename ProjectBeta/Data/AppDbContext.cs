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
            new User { Id = 1, Username = "admin", PasswordHash = "password" },
            new User { Id = 2, Username = "user1", PasswordHash = "password" }
        );
    }
}
