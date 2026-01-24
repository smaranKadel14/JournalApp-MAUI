using Microsoft.EntityFrameworkCore;
using JournalApp.Models;

namespace JournalApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tables
    public DbSet<User> Users => Set<User>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure EntryDate is always treated as "date-only" in our logic
        modelBuilder.Entity<JournalEntry>()
            .HasIndex(e => new { e.UserId, e.EntryDate })
            .IsUnique(); // Enforces ONLY one entry per user per day
    }
}
