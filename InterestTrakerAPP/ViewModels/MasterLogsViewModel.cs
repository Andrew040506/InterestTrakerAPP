using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels
{
    public partial class MasterLogsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        // The raw, unfiltered master list from the database
        private List<FinancialTransaction> _allMasterLogs = new();

        // The filtered list that the UI actually displays
        public ObservableCollection<FinancialTransaction> DisplayedLogs { get; } = new();

        // --- Filter Properties ---
        public List<string> FilterCategories { get; } = new()
        {
            "All Logs",
            "Ledger (In/Out)",
            "Portfolio (Trades)",
            "Goals (Savings)"
        };

        [ObservableProperty] private string _selectedCategory = "All Logs";
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private bool _isDateFilterActive = false;

        public MasterLogsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public void LoadLogs()
        {
            // Fetch everything from the zero-trust engine
            _allMasterLogs = _databaseService.GetAllTransactions();
            ApplyFilters();
        }

        // CommunityToolkit auto-triggers these methods when a property changes via UI bindings
        partial void OnSelectedCategoryChanged(string value) => ApplyFilters();
        partial void OnSelectedDateChanged(DateTime value) => ApplyFilters();
        partial void OnIsDateFilterActiveChanged(bool value) => ApplyFilters();

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedCategory = "All Logs";
            IsDateFilterActive = false;
            SelectedDate = DateTime.Today;
            // The property changes will automatically trigger ApplyFilters()
        }

        private void ApplyFilters()
        {
            var filtered = _allMasterLogs.AsEnumerable();

            // 1. Apply Type/Category Filter based on our exact database strings
            if (SelectedCategory == "Ledger (In/Out)")
            {
                filtered = filtered.Where(t => t.TransactionType == "Inflow" || t.TransactionType == "Outflow");
            }
            else if (SelectedCategory == "Portfolio (Trades)")
            {
                filtered = filtered.Where(t => t.TransactionType == "Buy" || t.TransactionType == "Sell");
            }
            else if (SelectedCategory == "Goals (Savings)")
            {
                filtered = filtered.Where(t => t.TransactionType == "GoalContribution");
            }

            // 2. Apply Date Filter (comparing just the Date component, ignoring the time)
            if (IsDateFilterActive)
            {
                filtered = filtered.Where(t => t.Timestamp.Date == SelectedDate.Date);
            }

            // 3. Push to UI
            DisplayedLogs.Clear();
            foreach (var item in filtered)
            {
                DisplayedLogs.Add(item);
            }
        }
    }
}