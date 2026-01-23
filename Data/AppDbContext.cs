using Microsoft.EntityFrameworkCore;
using JournalApp.Models;

namespace JournalApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Users table
    public DbSet<User> Users => Set<User>();

    // JournalEntries table
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
}
