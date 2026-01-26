using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty; // In a real app, you'd hash this!

    // Navigation
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}