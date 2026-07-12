using Microsoft.Extensions.Logging;
using InterestTrakerAPP.Services;
using InterestTrakerAPP.ViewModels;
using InterestTrakerAPP.Views;

namespace InterestTrakerAPP;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Register API Service
        builder.Services.AddSingleton<MarketApiService>();

        // 2. Register ViewModel
        builder.Services.AddTransient<MarketWatchViewModel>();

        // 3. Register Page (If this is missing, the app crashes on startup!)
        builder.Services.AddTransient<MarketWatchPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}