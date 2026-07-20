using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
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

        [ObservableProperty] private ObservableCollection<LedgerAccount> _ledgerAccounts;
        [ObservableProperty] private LedgerAccount _selectedAccount;

        // ADDED: Properties for Dynamic Platform Selection
        [ObservableProperty] private ObservableCollection<string> _availablePlatforms;
        [ObservableProperty] private string _selectedPlatform;

        public AddHoldingViewModel(DatabaseService databaseService, MarketApiService apiService)
        {
            _databaseService = databaseService;
            _apiService = apiService;

            // 1. Setup Ledger Accounts
            var accounts = _databaseService.GetAllAccounts();
            accounts.Insert(0, new LedgerAccount { Id = 0, AccountName = "Cash on Hand (External)" });
            LedgerAccounts = new ObservableCollection<LedgerAccount>(accounts);
            SelectedAccount = LedgerAccounts.FirstOrDefault();

            // 2. Load Unique Existing Platforms from DB and seed defaults if empty
            var existingPlatforms = _databaseService.GetAllHoldings()
                                                   .Select(h => h.Platform)
                                                   .Where(p => !string.IsNullOrWhiteSpace(p))
                                                   .Distinct()
                                                   .ToList();
            if (!existingPlatforms.Any())
            {
                existingPlatforms.Add("Gotrade");
                existingPlatforms.Add("Binance");
            }

            AvailablePlatforms = new ObservableCollection<string>(existingPlatforms);
            AvailablePlatforms.Add("+ Add New Platform...");

            // Assign backing field directly to avoid triggering popup during construction
            _selectedPlatform = AvailablePlatforms.FirstOrDefault();
        }

        // ADDED: Intercepts the dropdown selection to prompt for new inputs safely
        partial void OnSelectedPlatformChanged(string value)
        {
            if (value == "+ Add New Platform...")
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    string newPlatform = await Shell.Current.DisplayPromptAsync(
                        "New Platform",
                        "Enter the name of the new broker or platform:",
                        "OK",
                        "Cancel",
                        "e.g., eToro"
                    );

                    if (!string.IsNullOrWhiteSpace(newPlatform))
                    {
                        string cleanPlatform = newPlatform.Trim();

                        // 1. THE FIX: Temporarily point the picker to a safe, existing item.
                        // This forces the Windows UI to release its lock on the "+ Add New..." index.
                        SelectedPlatform = AvailablePlatforms.FirstOrDefault();

                        // 2. Give the UI thread a tiny fraction of a second to visually process the jump.
                        await Task.Delay(50);

                        // 3. Now that the picker isn't locked on the bottom item, it is 100% safe to insert!
                        AvailablePlatforms.Insert(AvailablePlatforms.Count - 1, cleanPlatform);

                        // 4. Finally, select your newly added platform.
                        SelectedPlatform = cleanPlatform;
                    }
                    else
                    {
                        // Fallback to first platform item if cancelled
                        SelectedPlatform = AvailablePlatforms.FirstOrDefault();
                    }
                });
            }
        }

        [RelayCommand]
        private async Task SubmitTradeAsync()
        {
            // 1. Validate Input
            if (string.IsNullOrWhiteSpace(Symbol) || Units <= 0 || PricePerUnit <= 0)
            {
                await Shell.Current.DisplayAlert("Missing Info", "Please ensure all fields have valid numbers.", "OK");
                return;
            }

            if (SelectedAccount == null)
            {
                await Shell.Current.DisplayAlert("Missing Account", "Please select a funding account for this trade.", "OK");
                return;
            }

            // ADDED: Validate platform field integrity
            if (string.IsNullOrWhiteSpace(SelectedPlatform) || SelectedPlatform == "+ Add New Platform...")
            {
                await Shell.Current.DisplayAlert("Missing Platform", "Please select a valid platform or broker.", "OK");
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

            // 2. Validate API Data
            var livePriceCheck = await _apiService.GetLivePriceAsync(cleanSymbol);
            if (livePriceCheck == null || livePriceCheck == 0)
            {
                await Shell.Current.DisplayAlert("Invalid Symbol", $"API could not find data for '{cleanSymbol}'.", "OK");
                return;
            }

            // 3. Pre-Trade Checks (Isolated specifically by Symbol AND Platform location)
            var allHoldings = _databaseService.GetAllHoldings();
            var existingAsset = allHoldings.FirstOrDefault(h => h.Symbol == cleanSymbol && h.Platform == SelectedPlatform);

            if (TradeType == "Sell")
            {
                if (existingAsset == null || existingAsset.TotalUnits < Units)
                {
                    await Shell.Current.DisplayAlert("Trade Error", $"You do not own enough {cleanSymbol} on {SelectedPlatform} to execute this sale.", "OK");
                    return;
                }
            }

            // 4. Execute the Zero-Trust Trade
            string finalName = string.IsNullOrWhiteSpace(Name) ? cleanSymbol : Name;

            // UPDATED: Added final platform string context argument to engine executor
            _databaseService.ExecutePortfolioTrade(
                accountId: SelectedAccount.Id,
                symbol: cleanSymbol,
                name: finalName,
                assetClass: AssetClass,
                units: Units,
                pricePerUnit: PricePerUnit,
                tradeType: TradeType,
                platform: SelectedPlatform
            );

            // 5. Cleanup check: If you sold everything on this platform location, remove empty row
            if (TradeType == "Sell" && existingAsset != null)
            {
                if ((existingAsset.TotalUnits - Units) <= 0)
                {
                    _databaseService.DeleteHolding(existingAsset);
                }
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}