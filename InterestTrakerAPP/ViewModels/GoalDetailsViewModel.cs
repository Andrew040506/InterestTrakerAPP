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

        // Analytics Metrics
        [ObservableProperty] private string _requiredPaceText = "Calculating...";
        [ObservableProperty] private string _estimatedCompletionText = "Calculating...";

        // Deposit Entry Field
        [ObservableProperty] private decimal _depositAmount;

        // Visual List Data
        public ObservableCollection<FinancialTransaction> ContributionHistory { get; } = new();

        public GoalDetailsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public void LoadGoalDetails()
        {
            // 1. Fetch the master goals using the new synchronous engine
            var goals = _databaseService.GetAllGoals();
            ActiveGoal = goals.FirstOrDefault(g => g.Id == GoalId);

            if (ActiveGoal == null) return;

            // 2. Fetch the contributions securely using the new filter
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

            // 3. Calculate Target Pacing using the updated TargetDate property
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

            // 4. Calculate Predictive Forecasting
            if (contributions.Count >= 2)
            {
                // The database service sorts newest first. 
                // Therefore, the FIRST historical deposit is the LAST item in the list.
                // Updated to use the new Timestamp property from FinancialTransaction
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

            // 5. Execute the atomic transfer via the zero-trust engine
            _databaseService.ExecuteMoneyFlow(
                sourceAccountId: 1, // Assumes ID 1 is the main ledger account from our SeedTestData
                destAccountId: null,
                targetGoalId: GoalId,
                amount: DepositAmount,
                type: "GoalContribution",
                description: "Manual goal contribution"
            );

            // Clear the entry box
            DepositAmount = 0;

            // Recalculate the entire page and refresh the lists instantly
            LoadGoalDetails();
        }
    }
}