using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JournalApp.Models;

public class JournalEntry
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    // This is the "daily journal date" (only ONE entry per day per user)
    [Required]
    public DateTime EntryDate { get; set; } = DateTime.Today;

    // System timestamps (do NOT let user manually type these)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // We store primary mood (required in UI); you can expand later
    [Required]
    public string Mood { get; set; } = "Neutral";

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}
