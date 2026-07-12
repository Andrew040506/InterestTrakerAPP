using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

public partial class MarketWatchViewModel : ObservableObject
{
    private readonly MarketApiService _apiService;

    // Reverted back to private fields
    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _searchSymbolText = string.Empty;

    public ObservableCollection<AssetQuote> Watchlist { get; } = new();

    public MarketWatchViewModel(MarketApiService apiService)
    {
        _apiService = apiService;

        Watchlist.Add(new AssetQuote { Symbol = "AAPL" });
        Watchlist.Add(new AssetQuote { Symbol = "BTC-USD" });

        _ = LoadPricesAsync();
    }

    [RelayCommand]
    private async Task LoadPricesAsync()
    {
        IsRefreshing = true;

        foreach (var asset in Watchlist)
        {
            var freshPrice = await _apiService.GetLivePriceAsync(asset.Symbol);
            if (freshPrice.HasValue)
            {
                asset.Price = freshPrice.Value;
            }
        }

        var temporaryList = new List<AssetQuote>(Watchlist);
        Watchlist.Clear();
        foreach (var item in temporaryList)
        {
            Watchlist.Add(item);
        }

        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task AddSymbolAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchSymbolText)) return;

        var cleanSymbol = SearchSymbolText.Trim().ToUpper();

        if (Watchlist.Any(a => a.Symbol == cleanSymbol)) return;

        var newAsset = new AssetQuote { Symbol = cleanSymbol };
        Watchlist.Add(newAsset);

        SearchSymbolText = string.Empty;

        var price = await _apiService.GetLivePriceAsync(cleanSymbol);
        if (price.HasValue)
        {
            newAsset.Price = price.Value;

            int index = Watchlist.IndexOf(newAsset);
            Watchlist[index] = newAsset;
        }
    }
}