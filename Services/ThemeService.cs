using Microsoft.Maui.Storage;

namespace JournalApp.Services
{
    // This service stores the selected theme and notifies the UI when theme changes.
    // We use it in MainLayout to apply CSS class: theme-light / theme-dark.
    public class ThemeService
    {
        private const string PrefKey = "app_theme"; // saved in app preferences

        // Stored theme name: "light" or "dark"
        public string CurrentTheme { get; private set; } = "light";

        // This is what you put into class="" in MainLayout
        public string CssClass => CurrentTheme == "dark" ? "theme-dark" : "theme-light";

        // Blazor components can subscribe to this to refresh UI
        public event Action? OnChange;

        // Load saved theme when app starts
        public void Load()
        {
            var saved = Preferences.Get(PrefKey, "light");
            SetTheme(saved, saveToPrefs: false);
        }

        // Set theme from UI
        public void SetTheme(string? theme, bool saveToPrefs = true)
        {
            // Normalize input (in case we receive "theme-dark" etc.)
            theme = (theme ?? "").Trim().ToLowerInvariant();
            if (theme == "theme-dark") theme = "dark";
            if (theme == "theme-light") theme = "light";

            if (theme != "dark") theme = "light"; // default safety

            // If no real change, do nothing
            if (CurrentTheme == theme)
                return;

            CurrentTheme = theme;

            // Save for next app startup
            if (saveToPrefs)
                Preferences.Set(PrefKey, CurrentTheme);

            // Notify UI (MainLayout, Settings, etc.)
            OnChange?.Invoke();
        }
    }
}
