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

    // Daily entry date (only ONE entry per day per user)
    [Required]
    public DateTime EntryDate { get; set; } = DateTime.Today;

    // System timestamps (do NOT let user manually type these)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Primary mood group stored (Positive/Neutral/Negative)
    [Required]
    public string Mood { get; set; } = "Neutral";

    // ✅ NEW: store secondary moods as CSV in DB (e.g., "Sad,Anxious")
    public string SecondaryMoodsCsv { get; set; } = "";

    // ✅ NEW: store tags as CSV in DB (e.g., "Work,Study,Family")
    public string TagsCsv { get; set; } = "";

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}
