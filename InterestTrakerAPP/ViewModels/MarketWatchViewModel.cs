using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace InterestTrakerAPP.ViewModels
{
    public partial class MarketWatchViewModel : ObservableObject
    {
        private readonly MarketApiService _apiService;
        private readonly DatabaseService _databaseService;
        private decimal _livePhpRate = 1m;

        private readonly List<AssetQuote> _masterWatchlist = new();
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

            var localAssets = _databaseService.GetAllWatchlistAssets();
            _masterWatchlist.Clear();
            _masterWatchlist.AddRange(localAssets);

            RefreshVisibleWatchlist();

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

            RefreshVisibleWatchlist();
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task AddSymbolAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchSymbolText)) return;

            var cleanSymbol = SearchSymbolText.Trim().ToUpper();

            if (_masterWatchlist.Any(a => a.Symbol == cleanSymbol))
            {
                await Shell.Current.DisplayAlert("Duplicate", $"{cleanSymbol} is already in your watchlist.", "OK");
                return;
            }

            string category = cleanSymbol.StartsWith("BINANCE:") || cleanSymbol.StartsWith("COINBASE:") || cleanSymbol.StartsWith("KRAKEN:")
                ? "Crypto" : "Stocks";

            var livePriceCheck = await _apiService.GetLivePriceAsync(cleanSymbol);

            if (livePriceCheck == null || livePriceCheck == 0)
            {
                await Shell.Current.DisplayAlert("Invalid Symbol", $"API could not find data for '{cleanSymbol}'.", "OK");
                return;
            }

            var newAsset = new AssetQuote
            {
                Symbol = cleanSymbol,
                AssetClass = category,
                PriceUsd = livePriceCheck.Value,
                CurrentPhpRate = _livePhpRate
            };

            _databaseService.SaveWatchlistAsset(newAsset);

            _masterWatchlist.Add(newAsset);
            SearchSymbolText = string.Empty;
            RefreshVisibleWatchlist();
        }

        [RelayCommand]
        private void DeleteAsset(AssetQuote item)
        {
            if (item == null) return;

            _databaseService.DeleteWatchlistAsset(item);
            _masterWatchlist.Remove(item);
            RefreshVisibleWatchlist();
        }

        [RelayCommand]
        private async Task NavigateToTradeAsync(AssetQuote asset)
        {
            if (asset == null) return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "Symbol", asset.Symbol },
                { "Name", asset.Symbol },
                { "AssetClass", asset.AssetClass }
            };

            await Shell.Current.GoToAsync("AddHoldingPage", navigationParameters);
        }

        [RelayCommand]
        private void SetFilter(string category)
        {
            _activeFilter = category;
            RefreshVisibleWatchlist();
        }

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
}