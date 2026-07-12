using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

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
        _ = LoadAccountsAsync();
    }

    [RelayCommand]
    private async Task LoadAccountsAsync()
    {
        IsRefreshing = true;
        var accounts = await _databaseService.GetAccountsAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Accounts.Clear();
            decimal total = 0;
            foreach (var acc in accounts)
            {
                Accounts.Add(acc);
                total += acc.CurrentBalance; // Tally up master balance
            }
            TotalCashBalance = total;
            IsRefreshing = false;
        });
    }

    [RelayCommand]
    private async Task AddAccountAsync()
    {
        // Triggers a native text input popup
        string result = await Shell.Current.DisplayPromptAsync("New Account", "Enter account name (e.g. BDO, Binance Wallet):", "Create", "Cancel");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var newAccount = new LedgerAccount { Name = result.Trim() };
            await _databaseService.SaveAccountAsync(newAccount);
            await LoadAccountsAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAccountAsync(LedgerAccount account)
    {
        if (account == null) return;

        bool confirm = await Shell.Current.DisplayAlert("Delete Account", $"Are you sure you want to delete {account.Name}? This will permanently erase all its transactions.", "Yes, Delete", "Cancel");
        if (confirm)
        {
            await _databaseService.DeleteAccountAsync(account);
            await LoadAccountsAsync();
        }
    }

    [RelayCommand]
    private async Task NavigateToDetailsAsync(LedgerAccount account)
    {
        if (account == null) return;

        // Pass the specific Account ID to the upcoming Details Page
        var navParams = new Dictionary<string, object>
        {
            { "AccountId", account.Id },
            { "AccountName", account.Name }
        };

        await Shell.Current.GoToAsync("AccountDetailsPage", navParams);
    }
}