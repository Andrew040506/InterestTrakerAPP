using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;
using Microsoft.Maui.ApplicationModel;

namespace InterestTrakerAPP.ViewModels
{
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

        // Now uses TransactionDisplayItem for enriched account-name labels
        public ObservableCollection<TransactionDisplayItem> Transactions { get; } = new();

        public AccountDetailsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public void LoadData()
        {
            // 1. Load this specific account to get its latest balance securely
            var myAccount = _databaseService.GetAccount(AccountId);
            if (myAccount != null)
            {
                CurrentBalance = myAccount.Balance;
                OnPropertyChanged(nameof(DisplayBalance));
            }

            // 2. Load enriched display items so account names are resolved
            var allTx = _databaseService.GetLedgerTransactionDisplayItems(AccountId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Transactions.Clear();
                foreach (var tx in allTx) Transactions.Add(tx);
            });
        }

        [RelayCommand]
        private void SubmitTransaction()
        {
            if (Amount <= 0) return;

            // Execute the atomic transfer via the zero-trust engine
            // For manual ledger entries, we assume it's an external expense (destAccountId is null)
            _databaseService.ExecuteMoneyFlow(
                sourceAccountId: AccountId,
                destAccountId: null,
                targetGoalId: null,
                amount: Amount,
                type: TransactionType,
                description: Notes
            );

            // Reset form and reload list
            Amount = 0;
            Notes = string.Empty;
            LoadData();
        }

        [RelayCommand]
        private void DeleteTransaction(TransactionDisplayItem tx)
        {
            // ZERO-TRUST ENFORCEMENT:
            // Immutability means we NEVER delete records.
            // In a real audit-ready system, you would log a "Reversal" transaction instead.
            // (Leave this empty to enforce audit integrity!)
        }
    }
}