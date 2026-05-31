using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;

namespace ProjectBeta.Tests.Helpers;

public static class TestDbContext
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }
}
