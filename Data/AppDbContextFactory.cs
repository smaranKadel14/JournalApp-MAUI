using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JournalApp.Data
{
    // I use this only for EF Core migrations (design-time).
    // MAUI runtime uses FileSystem.AppDataDirectory in MauiProgram.cs.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // For migrations I use a simple local SQLite file path
            // (EF tools need something they can access at design-time)
            optionsBuilder.UseSqlite("Data Source=journal.migrations.db");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
