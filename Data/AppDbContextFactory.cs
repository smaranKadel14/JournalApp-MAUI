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

            // Use the same path as runtime for consistency
            // This ensures migrations and runtime use the same database
            optionsBuilder.UseSqlite("Data Source=journal.db");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}