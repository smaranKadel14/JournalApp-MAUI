using JournalApp.Data;
using JournalApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using System.Diagnostics;

// ✅ Add this for QuestPDF license
using QuestPDF.Infrastructure;

namespace JournalApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // ✅ IMPORTANT:
        // QuestPDF requires setting a license ONCE at startup.
        // Community license is free under QuestPDF terms (most student projects).
        QuestPDF.Settings.License = LicenseType.Community;

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Theme service (you used Singleton already)
        builder.Services.AddSingleton<ThemeService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        Debug.WriteLine($"DB PATH = {dbPath}");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Prefer Scoped if it uses DbContext
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<JournalEntryService>();

        builder.Services.AddScoped<AnalyticsService>();


        // ✅ PDF export service should be Scoped (it reads DB and creates file per request)
        builder.Services.AddScoped<PdfExportService>();

        var app = builder.Build();

        // Load theme at startup
        var theme = app.Services.GetRequiredService<ThemeService>();
        theme.Load();

        // Apply migrations at startup
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
