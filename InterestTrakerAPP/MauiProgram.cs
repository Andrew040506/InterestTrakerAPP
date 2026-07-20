using InterestTrakerAPP.Services;
using InterestTrakerAPP.ViewModels;
using InterestTrakerAPP.Views;
using Microsoft.Extensions.Logging;

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

#if DEBUG
        builder.Logging.AddDebug();
#endif


        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<MarketApiService>();

        builder.Services.AddSingleton<MarketWatchViewModel>();
        builder.Services.AddSingleton<PortfolioViewModel>();
        builder.Services.AddSingleton<LedgerViewModel>(); // NEW!

        builder.Services.AddTransient<AddHoldingViewModel>();

        builder.Services.AddSingleton<MarketWatchPage>();
        builder.Services.AddSingleton<PortfolioPage>();
        builder.Services.AddSingleton<LedgerPage>(); 

        builder.Services.AddTransient<AddHoldingPage>();
        builder.Services.AddTransient<AccountDetailsViewModel>();
        builder.Services.AddTransient<AccountDetailsPage>();

        builder.Services.AddSingleton<GoalsViewModel>();
        builder.Services.AddSingleton<GoalsPage>();


        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
       
        builder.Services.AddSingleton<Services.AuthService>();
        builder.Services.AddSingleton<Services.DatabaseService>();

        builder.Services.AddTransient<Views.LoginPage>();
        // Register the Goal Details Page and ViewModel
        builder.Services.AddTransient<Views.GoalDetailsPage>();
        builder.Services.AddTransient<ViewModels.GoalDetailsViewModel>();
        return builder.Build();
    }
}