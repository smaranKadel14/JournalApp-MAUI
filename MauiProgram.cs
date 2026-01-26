using JournalApp.Data;
using JournalApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using System.IO;

namespace JournalApp
{
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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ✅ 1) SQLite database file path (inside app storage)
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
            Console.WriteLine("DB PATH = " + dbPath);


            // ✅ 2) Register EF Core DbContext to use SQLite
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Your existing service
            builder.Services.AddSingleton<UserService>();

            // Journal entries CRUD helper (create/update/delete)
            builder.Services.AddScoped<JournalApp.Services.JournalEntryService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // I apply EF Core migrations automatically when the app starts
                db.Database.Migrate();
            }


            return app;
        }
    }
}
