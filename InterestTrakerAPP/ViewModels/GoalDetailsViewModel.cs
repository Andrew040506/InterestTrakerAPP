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
    [QueryProperty(nameof(GoalId), "GoalId")]
    public partial class GoalDetailsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty] private int _goalId;
        [ObservableProperty] private SavingsGoal? _activeGoal;

        // Automatically triggers loading as soon as Shell passes the GoalId query parameter
        partial void OnGoalIdChanged(int value)
        {
            if (value > 0)
            {
                LoadGoalDetails();
            }
        }

        // Analytics Metrics
        [ObservableProperty] private string _requiredPaceText = "Calculating...";
        [ObservableProperty] private string _estimatedCompletionText = "Calculating...";

        // Deposit Entry Field
        [ObservableProperty] private decimal _depositAmount;

        // Funding Source Picker Properties
        [ObservableProperty] private ObservableCollection<LedgerAccount> _availableAccounts = new();
        [ObservableProperty] private LedgerAccount? _selectedFundingAccount;

        // Visual List Data
        public ObservableCollection<FinancialTransaction> ContributionHistory { get; } = new();

        public GoalDetailsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public void LoadGoalDetails()
        {
            // 1. Fetch the master goals using the synchronous engine
            var goals = _databaseService.GetAllGoals();
            ActiveGoal = goals.FirstOrDefault(g => g.Id == GoalId);

            if (ActiveGoal == null) return;

            // 2. Load available ledger accounts for the funding source picker
            var accounts = _databaseService.GetAllAccounts();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvailableAccounts.Clear();
                foreach (var acc in accounts)
                {
                    AvailableAccounts.Add(acc);
                }

                // Default to the first account if none is currently selected
                if (SelectedFundingAccount == null && AvailableAccounts.Any())
                {
                    SelectedFundingAccount = AvailableAccounts.First();
                }
            });

            // 3. Fetch the contributions securely using the filter
            var contributions = _databaseService.GetGoalTransactions(GoalId);

            // Update the UI list
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ContributionHistory.Clear();
                foreach (var c in contributions)
                {
                    ContributionHistory.Add(c);
                }
            });

            // Using the updated CurrentBalance property
            decimal remainingAmount = ActiveGoal.TargetAmount - ActiveGoal.CurrentBalance;

            // 4. Calculate Target Pacing using the updated TargetDate property
            TimeSpan remainingTime = ActiveGoal.TargetDate - DateTime.Today;
            if (remainingAmount <= 0)
            {
                RequiredPaceText = "Goal Accomplished!";
                EstimatedCompletionText = "Completed!";
                return;
            }

            if (remainingTime.TotalDays > 0)
            {
                decimal dailyRate = remainingAmount / (decimal)remainingTime.TotalDays;
                RequiredPaceText = $"₱{dailyRate:N2} / day needed";
            }
            else
            {
                RequiredPaceText = "Deadline passed";
            }

            // 5. Calculate Predictive Forecasting
            if (contributions.Count >= 2)
            {
                // The database service sorts newest first. 
                // Therefore, the FIRST historical deposit is the LAST item in the list.
                DateTime firstDepositDate = contributions.Last().Timestamp;
                TimeSpan daysElapsed = DateTime.Now - firstDepositDate;
                double totalDays = Math.Max(1.0, daysElapsed.TotalDays);

                decimal totalSavedHistorically = contributions.Sum(c => c.Amount);
                decimal savingsVelocityPerDay = totalSavedHistorically / (decimal)totalDays;

                if (savingsVelocityPerDay > 0)
                {
                    int daysToFinish = (int)Math.Ceiling(remainingAmount / savingsVelocityPerDay);
                    DateTime projectedDate = DateTime.Now.AddDays(daysToFinish);
                    EstimatedCompletionText = $"{projectedDate:MMM dd, yyyy} ({daysToFinish} days left)";
                }
                else
                {
                    EstimatedCompletionText = "Awaiting steady deposits...";
                }
            }
            else
            {
                EstimatedCompletionText = "Add more historical entries to generate a forecast";
            }
        }

        [RelayCommand]
        private void AddDeposit()
        {
            if (DepositAmount <= 0 || ActiveGoal == null) return;

            // Safety check: ensure a funding account is chosen
            if (SelectedFundingAccount == null)
            {
                return;
            }

            // 6. Execute the atomic transfer using the selected account's actual ID
            _databaseService.ExecuteMoneyFlow(
                sourceAccountId: SelectedFundingAccount.Id,
                destAccountId: null,
                targetGoalId: GoalId,
                amount: DepositAmount,
                type: "GoalContribution",
                description: $"Contribution from {SelectedFundingAccount.AccountName}"
            );

            // Clear the entry box
            DepositAmount = 0;

            // Recalculate the entire page and refresh the lists instantly
            LoadGoalDetails();
        }
    }
}