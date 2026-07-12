using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

public partial class MarketWatchViewModel : ObservableObject
{
    private readonly MarketApiService _apiService;
    private readonly DatabaseService _databaseService;
    private decimal _livePhpRate = 1m;

    // The background brain
    private readonly List<AssetQuote> _masterWatchlist = new();

    // Tracks which tab is currently selected so refreshes don't break the UI
    private string _activeFilter = "All";

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _searchSymbolText = string.Empty;

    public ObservableCollection<AssetQuote> Watchlist { get; } = new();

    public MarketWatchViewModel(MarketApiService apiService, DatabaseService databaseService)
    {
        _apiService = apiService;
        _databaseService = databaseService;

        _ = LoadPricesAsync();
    }

    [RelayCommand]
    private async Task LoadPricesAsync()
    {
        IsRefreshing = true;

        // 1. Load permanent assets from SQLite database immediately
        var localAssets = await _databaseService.GetWatchlistAsync();
        _masterWatchlist.Clear();
        _masterWatchlist.AddRange(localAssets);

        // Update UI immediately so it's not blank while loading prices
        RefreshVisibleWatchlist();

        // 2. Fetch live prices
        _livePhpRate = await _apiService.GetUsdToPhpRateAsync();

        foreach (var asset in _masterWatchlist)
        {
            var freshPrice = await _apiService.GetLivePriceAsync(asset.Symbol);
            if (freshPrice.HasValue)
            {
                asset.PriceUsd = freshPrice.Value;
                asset.CurrentPhpRate = _livePhpRate;
            }
        }

        // 3. Refresh UI again with the new prices
        RefreshVisibleWatchlist();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task AddSymbolAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchSymbolText)) return;

        var cleanSymbol = SearchSymbolText.Trim().ToUpper();

        // 1. Prevent adding duplicates
        if (_masterWatchlist.Any(a => a.Symbol == cleanSymbol))
        {
            await Shell.Current.DisplayAlert("Duplicate", $"{cleanSymbol} is already in your watchlist.", "OK");
            return;
        }

        // 2. AUTO-CORRECT ASSET CLASS
        string category = cleanSymbol.StartsWith("BINANCE:") || cleanSymbol.StartsWith("COINBASE:") || cleanSymbol.StartsWith("KRAKEN:")
            ? "Crypto" : "Stocks";

        // 3. THE API SAFETY NET
        var livePriceCheck = await _apiService.GetLivePriceAsync(cleanSymbol);

        if (livePriceCheck == null || livePriceCheck == 0)
        {
            await Shell.Current.DisplayAlert("Invalid Symbol",
                $"The API could not find live data for '{cleanSymbol}'. Please check your spelling.",
                "OK");
            return;
        }

        // 4. Create the verified asset (We use the price we just fetched so it loads instantly!)
        var newAsset = new AssetQuote
        {
            Symbol = cleanSymbol,
            AssetClass = category,
            PriceUsd = livePriceCheck.Value,
            CurrentPhpRate = _livePhpRate
        };

        // 5. Save to Database permanently
        await _databaseService.SaveWatchlistAssetAsync(newAsset);

        _masterWatchlist.Add(newAsset);
        SearchSymbolText = string.Empty;
        RefreshVisibleWatchlist();
    }

    // NEW: Swipe-to-Delete functionality
    [RelayCommand]
    private async Task DeleteAssetAsync(AssetQuote item)
    {
        if (item == null) return;

        // Delete from Database
        await _databaseService.DeleteWatchlistAssetAsync(item);

        // Remove from Master List and UI
        _masterWatchlist.Remove(item);
        RefreshVisibleWatchlist();
    }

    [RelayCommand]
    private void SetFilter(string category)
    {
        _activeFilter = category;
        RefreshVisibleWatchlist();
    }

    // This method now respects the active filter!
    private void RefreshVisibleWatchlist()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Watchlist.Clear();
            var filteredList = _activeFilter == "All"
                ? _masterWatchlist
                : _masterWatchlist.Where(a => a.AssetClass == _activeFilter);

            foreach (var item in filteredList)
            {
                Watchlist.Add(item);
            }
        });
    }
}