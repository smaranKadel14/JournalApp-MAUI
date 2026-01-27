using JournalApp.Data;
using JournalApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace JournalApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Theme must be Singleton
        builder.Services.AddSingleton<ThemeService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // SQLite file in AppDataDirectory
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        Debug.WriteLine($"DB PATH = {dbPath}");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // User session should be Singleton (shared across whole app)
        builder.Services.AddSingleton<UserService>();

        // Uses DbContext -> Scoped
        builder.Services.AddScoped<JournalEntryService>();

        var app = builder.Build();

        // Load saved theme at startup
        var theme = app.Services.GetRequiredService<ThemeService>();
        theme.Load();

        // Apply migrations safely
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Migration failed: " + ex);
        }

        return app;
    }
}
