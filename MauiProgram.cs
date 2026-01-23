using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; 
using JournalApp.Data;          
using JournalApp.Models;           

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

            var connectionString = "Host=localhost;Port=5432;Database=JournalDB;Username=postgres;Password=test123";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

#if DEBUG
            
            builder.Services.AddSingleton<JournalApp.Services.UserService>();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}