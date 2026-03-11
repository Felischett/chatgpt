using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Messanger.Services;
using Messanger.ViewModels;
using Messanger.Views;

namespace Messanger
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // === Services ===
            // Backend Base URL (wie bei dir im ApiService)
            builder.Services.AddSingleton(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7246/")
            });

            builder.Services.AddSingleton<TranslationService>();
            // === Pages ===
            builder.Services.AddSingleton<SettingsPage>();

            // === ViewModels (für Settings) ===
            builder.Services.AddSingleton<SettingsViewModel>();

            return builder.Build();
        }
    }
}