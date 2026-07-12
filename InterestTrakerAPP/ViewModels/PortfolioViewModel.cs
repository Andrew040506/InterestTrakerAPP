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

    public string DisplayTotalValue => $"₱{TotalPortfolioValue:N2}";
    public string DisplayUnrealizedPnL => $"{(TotalUnrealizedPnL >= 0 ? "+" : "")}₱{TotalUnrealizedPnL:N2}";
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

        var localHoldings = await _databaseService.GetHoldingsAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Holdings.Clear();
            foreach (var holding in localHoldings)
            {
                Holdings.Add(holding);
            }
        });

        decimal phpRate = await _apiService.GetUsdToPhpRateAsync();
        decimal tempTotalValue = 0;
        decimal tempTotalPnL = 0;

        foreach (var asset in Holdings)
        {
            var liveUsdPrice = await _apiService.GetLivePriceAsync(asset.Symbol);
            if (liveUsdPrice.HasValue)
            {
                asset.LivePrice = liveUsdPrice.Value * phpRate;
            }
            else if (asset.LivePrice == 0)
            {
                asset.LivePrice = asset.AverageBuyPrice;
            }

            tempTotalValue += asset.TotalValue;
            tempTotalPnL += asset.UnrealizedPnL;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            TotalPortfolioValue = tempTotalValue;
            TotalUnrealizedPnL = tempTotalPnL;
            IsRefreshing = false;
        });
    }

    // Swipe-to-Delete functionality is kept here!
    [RelayCommand]
    private async Task DeleteHoldingAsync(PortfolioItem item)
    {
        if (item == null) return;

        await _databaseService.DeleteHoldingAsync(item);
        Holdings.Remove(item);

        // Recalculate totals
        _ = LoadPortfolioDataAsync();
    }
}