using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JournalApp.Data
{
    // This class is ONLY for EF migrations (design-time).
    // It tells EF how to create AppDbContext when running Add-Migration / Update-Database.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Use a simple DB path for migrations time (project folder).
            // This does NOT affect the runtime MAUI DB path unless you also change it there.
            var dbPath = Path.Combine(AppContext.BaseDirectory, "journal.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
