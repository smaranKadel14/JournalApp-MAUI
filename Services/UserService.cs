namespace JournalApp.Services;

public class UserService
{
    // Holds the logged-in user's database ID (null means "not logged in")
    public int? CurrentUserId { get; private set; }

    // Holds the logged-in user's username for showing in UI (Home page, etc.)
    public string CurrentUsername { get; private set; } = "";

    // True when we have a valid logged-in user
    public bool IsLoggedIn => CurrentUserId.HasValue;

    // Call this after a successful login to store session details in memory
    public void SetCurrentUser(int userId, string username)
    {
        CurrentUserId = userId;
        CurrentUsername = username;
    }

    // Convenience overload: if you only have the ID, still allow setting the session
    public void SetCurrentUser(int userId)
    {
        CurrentUserId = userId;
        CurrentUsername = "";
    }

    // Clears the session when the user logs out
    public void Logout()
    {
        CurrentUserId = null;
        CurrentUsername = "";
    }
}
