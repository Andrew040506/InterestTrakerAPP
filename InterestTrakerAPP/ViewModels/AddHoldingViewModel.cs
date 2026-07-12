using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

public partial class AddHoldingViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty] private string _symbol = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _assetClass = "Stocks";

    // NEW: Buy or Sell toggle
    [ObservableProperty] private string _tradeType = "Buy";

    [ObservableProperty] private decimal _units;
    [ObservableProperty] private decimal _pricePerUnit;

    public AddHoldingViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    private async Task SubmitTradeAsync()
    {
        if (string.IsNullOrWhiteSpace(Symbol) || Units <= 0 || PricePerUnit <= 0) return;

        var cleanSymbol = Symbol.ToUpper().Trim();

        // 1. Log the receipt in our new Transaction Database
        var transaction = new TradeTransaction
        {
            Symbol = cleanSymbol,
            TradeType = TradeType,
            Units = Units,
            PricePerUnit = PricePerUnit,
            TradeDate = DateTime.Now
        };
        await _databaseService.SaveTransactionAsync(transaction);

        // 2. Do the Math for the Portfolio Dashboard
        var allHoldings = await _databaseService.GetHoldingsAsync();
        var existingAsset = allHoldings.FirstOrDefault(h => h.Symbol == cleanSymbol);

        if (TradeType == "Buy")
        {
            if (existingAsset != null)
            {
                // Asset exists: Calculate the new Blended Average Cost
                decimal totalCurrentCost = existingAsset.TotalUnits * existingAsset.AverageBuyPrice;
                decimal totalNewCost = Units * PricePerUnit;

                existingAsset.TotalUnits += Units;
                existingAsset.AverageBuyPrice = (totalCurrentCost + totalNewCost) / existingAsset.TotalUnits;

                await _databaseService.SaveHoldingAsync(existingAsset);
            }
            else
            {
                // Brand new asset
                var newAsset = new PortfolioItem
                {
                    Symbol = cleanSymbol,
                    Name = Name,
                    AssetClass = AssetClass,
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
                    // You sold everything! Auto-delete it from the dashboard.
                    await _databaseService.DeleteHoldingAsync(existingAsset);
                }
                else
                {
                    // You still have some units left, save the updated amount.
                    await _databaseService.SaveHoldingAsync(existingAsset);
                }
            }
        }

        // 3. Return to the Dashboard
        await Shell.Current.GoToAsync("..");
    }
}