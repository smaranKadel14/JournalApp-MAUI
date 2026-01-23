namespace JournalApp.Services;

public class UserService
{
    // This will hold the ID of the person who just logged in
    public int CurrentUserId { get; set; }
    public string CurrentUsername { get; set; } = string.Empty;
}