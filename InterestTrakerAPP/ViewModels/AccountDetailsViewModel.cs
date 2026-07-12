using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

[QueryProperty(nameof(AccountId), "AccountId")]
[QueryProperty(nameof(AccountName), "AccountName")]
public partial class AccountDetailsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    // Passed in from the Ledger Page
    [ObservableProperty] private int _accountId;
    [ObservableProperty] private string _accountName = string.Empty;

    [ObservableProperty] private decimal _currentBalance;
    public string DisplayBalance => $"₱{CurrentBalance:N2}";

    // Form Inputs
    [ObservableProperty] private string _transactionType = "Inflow";
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string _notes = string.Empty;

    public ObservableCollection<LedgerTransaction> Transactions { get; } = new();

    public AccountDetailsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        // 1. Load this specific account to get its latest balance
        var accounts = await _databaseService.GetAccountsAsync();
        var myAccount = accounts.FirstOrDefault(a => a.Id == AccountId);
        if (myAccount != null)
        {
            CurrentBalance = myAccount.CurrentBalance;
            OnPropertyChanged(nameof(DisplayBalance));
        }

        // 2. Load all transactions using our new legal public method!
        var allTx = await _databaseService.GetLedgerTransactionsAsync(AccountId);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Transactions.Clear();
            foreach (var tx in allTx) Transactions.Add(tx);
        });
    }

    [RelayCommand]
    private async Task SubmitTransactionAsync()
    {
        if (Amount <= 0) return;

        var tx = new LedgerTransaction
        {
            AccountId = AccountId,
            Type = TransactionType,
            Amount = Amount,
            Notes = Notes,
            Timestamp = DateTime.Now
        };

        await _databaseService.SaveLedgerTransactionAsync(tx);

        // Reset form and reload list
        Amount = 0;
        Notes = string.Empty;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(LedgerTransaction tx)
    {
        if (tx == null) return;
        await _databaseService.DeleteLedgerTransactionAsync(tx);
        await LoadDataAsync();
    }
}