using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

public partial class PortfolioViewModel : ObservableObject
{
    private readonly MarketApiService _apiService;
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTotalValue))]
    private decimal _totalPortfolioValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayUnrealizedPnL))]
    [NotifyPropertyChangedFor(nameof(IsOverallProfitable))]
    private decimal _totalUnrealizedPnL;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayCashBalance))]
    private decimal _cashBalance = 85400.00m;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayRealizedPnL))]
    private decimal _realizedPnL = 24100.00m;

    public string DisplayTotalValue => $"₱{TotalPortfolioValue:N2}";
    public string DisplayUnrealizedPnL => $"{(TotalUnrealizedPnL >= 0 ? "+" : "")}₱{TotalUnrealizedPnL:N2}";
    public string DisplayCashBalance => $"₱{CashBalance:N2}";
    public string DisplayRealizedPnL => $"₱{RealizedPnL:N2}";
    public bool IsOverallProfitable => TotalUnrealizedPnL >= 0;

    public ObservableCollection<PortfolioItem> Holdings { get; } = new();

    public PortfolioViewModel(MarketApiService apiService, DatabaseService databaseService)
    {
        _apiService = apiService;
        _databaseService = databaseService;

        _ = LoadPortfolioDataAsync();
    }

    [RelayCommand]
    private async Task LoadPortfolioDataAsync()
    {
        IsRefreshing = true;

        // 1. FETCH FROM YOUR LOCAL SQLITE DATABASE
        var localHoldings = await _databaseService.GetHoldingsAsync();

        // Update the UI collection safely on the Main Thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Holdings.Clear();
            foreach (var holding in localHoldings)
            {
                Holdings.Add(holding);
            }
        });

        // 2. Fetch Live PHP Conversion Rate
        decimal phpRate = await _apiService.GetUsdToPhpRateAsync();

        decimal tempTotalValue = 0;
        decimal tempTotalPnL = 0;

        // 3. Fetch Live Prices for Holdings and Calculate Math
        foreach (var asset in Holdings)
        {
            var liveUsdPrice = await _apiService.GetLivePriceAsync(asset.Symbol);
            if (liveUsdPrice.HasValue)
            {
                asset.LivePrice = liveUsdPrice.Value * phpRate;
            }
            else if (asset.LivePrice == 0)
            {
                // Fallback if the API is unreachable
                asset.LivePrice = asset.AverageBuyPrice;
            }

            tempTotalValue += asset.TotalValue;
            tempTotalPnL += asset.UnrealizedPnL;
        }

        // 4. Update the Master Dashboard Numbers on the Main Thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TotalPortfolioValue = tempTotalValue;
            TotalUnrealizedPnL = tempTotalPnL;
            IsRefreshing = false;
        });
    }
}