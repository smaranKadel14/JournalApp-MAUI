using Microsoft.Maui.Storage;

namespace JournalApp.Services;

public class ThemeService
{
    private const string Key = "app_theme"; // light | dark

    public string CurrentTheme { get; private set; } = "light";
    public event Action? OnChange;

    public void Load()
    {
        CurrentTheme = Preferences.Default.Get(Key, "light");
        OnChange?.Invoke();
    }

    public void SetTheme(string theme)
    {
        if (theme != "light" && theme != "dark") theme = "light";
        if (CurrentTheme == theme) return;

        CurrentTheme = theme;
        Preferences.Default.Set(Key, theme);
        OnChange?.Invoke();
    }

    public string CssClass => CurrentTheme == "dark" ? "theme-dark" : "theme-light";
}
