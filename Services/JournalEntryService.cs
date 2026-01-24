using Microsoft.EntityFrameworkCore;
using JournalApp.Data;
using JournalApp.Models;

namespace JournalApp.Services;

public class JournalEntryService
{
    private readonly AppDbContext _db;

    public JournalEntryService(AppDbContext db)
    {
        _db = db;
    }

    // Gets an entry for a specific user + date (date-only comparison)
    public Task<JournalEntry?> GetByDateAsync(int userId, DateTime date)
    {
        var day = date.Date;
        return _db.JournalEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && e.EntryDate == day);
    }

    // Gets a specific entry by ID (used for edit from dashboard)
    public Task<JournalEntry?> GetByIdAsync(int userId, int entryId)
    {
        return _db.JournalEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == entryId);
    }

    // Creates or updates (Upsert) while enforcing "one per day"
    public async Task<JournalEntry> UpsertAsync(JournalEntry incoming)
    {
        incoming.EntryDate = incoming.EntryDate.Date;     // normalize to date-only
        var now = DateTime.UtcNow;

        // If it's a new entry, check if that day already exists
        if (incoming.Id == 0)
        {
            var existing = await GetByDateAsync(incoming.UserId, incoming.EntryDate);

            if (existing != null)
            {
                // Convert create into update (prevents duplicates)
                existing.Title = incoming.Title;
                existing.Content = incoming.Content;
                existing.Mood = incoming.Mood;
                existing.UpdatedAt = now;

                await _db.SaveChangesAsync();
                return existing;
            }

            // Brand new entry
            incoming.CreatedAt = now;
            incoming.UpdatedAt = now;

            _db.JournalEntries.Add(incoming);
            await _db.SaveChangesAsync();
            return incoming;
        }

        // Update existing entry by Id (edit mode)
        var toUpdate = await GetByIdAsync(incoming.UserId, incoming.Id)
            ?? throw new InvalidOperationException("Entry not found.");

        toUpdate.Title = incoming.Title;
        toUpdate.Content = incoming.Content;
        toUpdate.Mood = incoming.Mood;

        // EntryDate should also be editable, but still date-only
        toUpdate.EntryDate = incoming.EntryDate.Date;

        // System update timestamp
        toUpdate.UpdatedAt = now;

        await _db.SaveChangesAsync();
        return toUpdate;
    }

    // Deletes an entry owned by the logged-in user
    public async Task DeleteAsync(int userId, int entryId)
    {
        var entry = await GetByIdAsync(userId, entryId);
        if (entry == null) return;

        _db.JournalEntries.Remove(entry);
        await _db.SaveChangesAsync();
    }
}
