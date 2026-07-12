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
        
        builder.Services.AddSingleton<MarketApiService>();       
        builder.Services.AddSingleton<DatabaseService>(); 
        builder.Services.AddTransient<MarketWatchViewModel>();
        builder.Services.AddTransient<MarketWatchPage>();
        builder.Services.AddTransient<PortfolioViewModel>();
        builder.Services.AddTransient<PortfolioPage>();
        builder.Services.AddTransient<AddHoldingViewModel>();
        builder.Services.AddTransient<AddHoldingPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}