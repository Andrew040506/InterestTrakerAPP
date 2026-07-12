using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

public partial class MarketWatchViewModel : ObservableObject
{
    private readonly MarketApiService _apiService;
    private decimal _livePhpRate = 1m;

    // Holds ALL assets in the background
    private readonly List<AssetQuote> _masterWatchlist = new();

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _searchSymbolText = string.Empty;

    // What the UI actually displays based on the active filter
    public ObservableCollection<AssetQuote> Watchlist { get; } = new();

    public MarketWatchViewModel(MarketApiService apiService)
    {
        _apiService = apiService;

        // Setup initial default assets
        AddAssetToMasterList("AAPL", "Stocks");
        AddAssetToMasterList("BINANCE:BTCUSDT", "Crypto");

        _ = LoadPricesAsync();
    }

    private void AddAssetToMasterList(string symbol, string assetClass)
    {
        var asset = new AssetQuote { Symbol = symbol, AssetClass = assetClass };
        _masterWatchlist.Add(asset);
        Watchlist.Add(asset);
    }

    [RelayCommand]
    private async Task LoadPricesAsync()
    {
        IsRefreshing = true;

        // 1. Fetch the latest PHP conversion rate first
        _livePhpRate = await _apiService.GetUsdToPhpRateAsync();

        // 2. Fetch prices for everything in the background
        foreach (var asset in _masterWatchlist)
        {
            var freshPrice = await _apiService.GetLivePriceAsync(asset.Symbol);
            if (freshPrice.HasValue)
            {
                asset.PriceUsd = freshPrice.Value;
                asset.CurrentPhpRate = _livePhpRate;
            }
        }

        // 3. Force the UI to redraw the current visible list
        RefreshVisibleWatchlist();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task AddSymbolAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchSymbolText)) return;

        var cleanSymbol = SearchSymbolText.Trim().ToUpper();
        if (_masterWatchlist.Any(a => a.Symbol == cleanSymbol)) return;

        // Automatically categorize it based on standard crypto exchange prefixes
        string category = cleanSymbol.Contains("BINANCE:") || cleanSymbol.Contains("COINBASE:") ? "Crypto" : "Stocks";

        var newAsset = new AssetQuote
        {
            Symbol = cleanSymbol,
            AssetClass = category,
            CurrentPhpRate = _livePhpRate
        };

        _masterWatchlist.Add(newAsset);
        Watchlist.Add(newAsset);
        SearchSymbolText = string.Empty;

        // Fetch live price immediately
        var price = await _apiService.GetLivePriceAsync(cleanSymbol);
        if (price.HasValue)
        {
            newAsset.PriceUsd = price.Value;
            RefreshVisibleWatchlist();
        }
    }

    // NEW: Handle the Category Chip Clicks
    [RelayCommand]
    private void SetFilter(string category)
    {
        Watchlist.Clear();
        var filteredList = category == "All"
            ? _masterWatchlist
            : _masterWatchlist.Where(a => a.AssetClass == category);

        foreach (var item in filteredList)
        {
            Watchlist.Add(item);
        }
    }

    private void RefreshVisibleWatchlist()
    {
        var currentItems = new List<AssetQuote>(Watchlist);
        Watchlist.Clear();
        foreach (var item in currentItems)
        {
            Watchlist.Add(item);
        }
    }
}