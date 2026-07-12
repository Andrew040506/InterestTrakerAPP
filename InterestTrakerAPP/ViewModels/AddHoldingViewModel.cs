using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

// These tell MAUI to automatically fill these properties when navigating from the Explorer
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
        // 1. Basic empty-field validation
        if (string.IsNullOrWhiteSpace(Symbol) || Units <= 0 || PricePerUnit <= 0)
        {
            await Shell.Current.DisplayAlert("Missing Info", "Please ensure all fields have valid numbers and a symbol is entered.", "OK");
            return;
        }

        var cleanSymbol = Symbol.ToUpper().Trim();

        // 2. AUTO-CORRECT ASSET CLASS
        // If it has a crypto exchange prefix, force it to Crypto. Otherwise, it's a Stock.
        if (cleanSymbol.StartsWith("BINANCE:") || cleanSymbol.StartsWith("COINBASE:") || cleanSymbol.StartsWith("KRAKEN:"))
        {
            AssetClass = "Crypto";
        }
        else
        {
            AssetClass = "Stocks";
        }

        // 3. THE API SAFETY NET
        var livePriceCheck = await _apiService.GetLivePriceAsync(cleanSymbol);

        if (livePriceCheck == null || livePriceCheck == 0)
        {
            await Shell.Current.DisplayAlert("Invalid Symbol",
                $"The API could not find live data for '{cleanSymbol}'. Please check your spelling and formatting.",
                "OK");
            return;
        }

        // 4. Log the receipt in our Transaction Database
        var transaction = new TradeTransaction
        {
            Symbol = cleanSymbol,
            TradeType = TradeType,
            Units = Units,
            PricePerUnit = PricePerUnit,
            TradeDate = DateTime.Now
        };
        await _databaseService.SaveTransactionAsync(transaction);

        // 5. Do the Math for the Portfolio Dashboard
        var allHoldings = await _databaseService.GetHoldingsAsync();
        var existingAsset = allHoldings.FirstOrDefault(h => h.Symbol == cleanSymbol);

        if (TradeType == "Buy")
        {
            if (existingAsset != null)
            {
                decimal totalCurrentCost = existingAsset.TotalUnits * existingAsset.AverageBuyPrice;
                decimal totalNewCost = Units * PricePerUnit;

                existingAsset.TotalUnits += Units;
                existingAsset.AverageBuyPrice = (totalCurrentCost + totalNewCost) / existingAsset.TotalUnits;

                await _databaseService.SaveHoldingAsync(existingAsset);
            }
            else
            {
                string finalName = string.IsNullOrWhiteSpace(Name) ? cleanSymbol : Name;

                var newAsset = new PortfolioItem
                {
                    Symbol = cleanSymbol,
                    Name = finalName,
                    AssetClass = AssetClass, // This now uses the auto-corrected version!
                    TotalUnits = Units,
                    AverageBuyPrice = PricePerUnit
                };
                await _databaseService.SaveHoldingAsync(newAsset);
            }
        }
        else if (TradeType == "Sell")
        {
            if (existingAsset != null)
            {
                existingAsset.TotalUnits -= Units;

                if (existingAsset.TotalUnits <= 0)
                {
                    await _databaseService.DeleteHoldingAsync(existingAsset);
                }
                else
                {
                    await _databaseService.SaveHoldingAsync(existingAsset);
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", $"You do not own any {cleanSymbol} to sell.", "OK");
                return;
            }
        }

        // 6. Return to the Dashboard
        await Shell.Current.GoToAsync("..");
    }
}