using System;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;
using Microsoft.Maui.Controls;

namespace InterestTrakerAPP.ViewModels
{
    [QueryProperty(nameof(Symbol), "Symbol")]
    [QueryProperty(nameof(Name), "Name")]
    [QueryProperty(nameof(AssetClass), "AssetClass")]
    public partial class AddHoldingViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly MarketApiService _apiService;

        [ObservableProperty] private string _symbol = string.Empty;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _assetClass = "Stocks";
        [ObservableProperty] private string _tradeType = "Buy";
        [ObservableProperty] private decimal _units;
        [ObservableProperty] private decimal _pricePerUnit;

        public AddHoldingViewModel(DatabaseService databaseService, MarketApiService apiService)
        {
            _databaseService = databaseService;
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task SubmitTradeAsync()
        {
            if (string.IsNullOrWhiteSpace(Symbol) || Units <= 0 || PricePerUnit <= 0)
            {
                await Shell.Current.DisplayAlert("Missing Info", "Please ensure all fields have valid numbers.", "OK");
                return;
            }

            var cleanSymbol = Symbol.ToUpper().Trim();

            if (cleanSymbol.StartsWith("BINANCE:") || cleanSymbol.StartsWith("COINBASE:") || cleanSymbol.StartsWith("KRAKEN:"))
            {
                AssetClass = "Crypto";
            }
            else
            {
                AssetClass = "Stocks";
            }

            var livePriceCheck = await _apiService.GetLivePriceAsync(cleanSymbol);

            if (livePriceCheck == null || livePriceCheck == 0)
            {
                await Shell.Current.DisplayAlert("Invalid Symbol", $"API could not find data for '{cleanSymbol}'.", "OK");
                return;
            }

            // Route the trade cost through the immutable Financial Transaction log!
            _databaseService.ExecuteMoneyFlow(
                sourceAccountId: 1, // Assumes main ledger is funding this trade
                destAccountId: null,
                targetGoalId: null,
                amount: Units * PricePerUnit,
                type: "PortfolioTrade",
                description: $"{TradeType} {Units} of {cleanSymbol} @ ${PricePerUnit}"
            );

            var allHoldings = _databaseService.GetAllHoldings();
            var existingAsset = allHoldings.FirstOrDefault(h => h.Symbol == cleanSymbol);

            if (TradeType == "Buy")
            {
                if (existingAsset != null)
                {
                    decimal totalCurrentCost = existingAsset.TotalUnits * existingAsset.AverageBuyPrice;
                    decimal totalNewCost = Units * PricePerUnit;

                    existingAsset.TotalUnits += Units;
                    existingAsset.AverageBuyPrice = (totalCurrentCost + totalNewCost) / existingAsset.TotalUnits;

                    _databaseService.SaveHolding(existingAsset);
                }
                else
                {
                    string finalName = string.IsNullOrWhiteSpace(Name) ? cleanSymbol : Name;

                    var newAsset = new PortfolioItem
                    {
                        Symbol = cleanSymbol,
                        Name = finalName,
                        AssetClass = AssetClass,
                        TotalUnits = Units,
                        AverageBuyPrice = PricePerUnit
                    };
                    _databaseService.SaveHolding(newAsset);
                }
            }
            else if (TradeType == "Sell")
            {
                if (existingAsset != null)
                {
                    existingAsset.TotalUnits -= Units;

                    if (existingAsset.TotalUnits <= 0)
                    {
                        _databaseService.DeleteHolding(existingAsset);
                    }
                    else
                    {
                        _databaseService.SaveHolding(existingAsset);
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", $"You do not own any {cleanSymbol} to sell.", "OK");
                    return;
                }
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}