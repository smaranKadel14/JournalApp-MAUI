namespace JournalApp.Services
{
    public class UserService
    {
        // I keep session data here
        public bool IsLoggedIn { get; private set; }
        public int? CurrentUserId { get; private set; }
        public string? CurrentUsername { get; private set; }

        // I use this event to tell UI to refresh when login state changes
        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        // I call this after successful login
        public void SetCurrentUser(int userId, string username)
        {
            CurrentUserId = userId;
            CurrentUsername = username;
            IsLoggedIn = true;

            NotifyStateChanged();
        }

        // I call this when user logs out
        public void Logout()
        {
            CurrentUserId = null;
            CurrentUsername = null;
            IsLoggedIn = false;

            NotifyStateChanged();
        }
    }
}
