using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace InterestTrakerAPP.ViewModels
{
    public partial class LedgerViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty] private bool _isRefreshing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayTotalBalance))]
        private decimal _totalCashBalance;

        public string DisplayTotalBalance => $"₱{TotalCashBalance:N2}";

        public ObservableCollection<LedgerAccount> Accounts { get; } = new();

        public LedgerViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoadAccounts();
        }

        [RelayCommand]
        public void LoadAccounts()
        {
            IsRefreshing = true;

            // Synchronous zero-trust engine call
            var accounts = _databaseService.GetAllAccounts();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Accounts.Clear();
                decimal total = 0;
                foreach (var acc in accounts)
                {
                    Accounts.Add(acc);
                    total += acc.Balance; // Updated to new Balance property
                }
                TotalCashBalance = total;
                IsRefreshing = false;
            });
        }

        [RelayCommand]
        private async Task AddAccountAsync()
        {
            string result = await Shell.Current.DisplayPromptAsync("New Account", "Enter account name (e.g. BDO, Binance Wallet):", "Create", "Cancel");

            if (!string.IsNullOrWhiteSpace(result))
            {
                var newAccount = new LedgerAccount { AccountName = result.Trim(), Balance = 0, IsActive = true };
                _databaseService.SaveAccount(newAccount);
                LoadAccounts();
            }
        }

        [RelayCommand]
        private async Task DeleteAccountAsync(LedgerAccount account)
        {
            if (account == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete Account", $"Are you sure you want to delete {account.AccountName}? This will permanently erase all its transactions.", "Yes, Delete", "Cancel");
            if (confirm)
            {
                _databaseService.DeleteAccount(account);
                LoadAccounts();
            }
        }

        [RelayCommand]
        private async Task NavigateToDetailsAsync(LedgerAccount account)
        {
            if (account == null) return;

            var navParams = new Dictionary<string, object>
            {
                { "AccountId", account.Id },
                { "AccountName", account.AccountName }
            };

            await Shell.Current.GoToAsync("AccountDetailsPage", navParams);
        }
    }
}