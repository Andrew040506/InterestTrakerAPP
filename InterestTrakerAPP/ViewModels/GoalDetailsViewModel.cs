using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

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
    public ObservableCollection<GoalContribution> ContributionHistory { get; } = new();

    public GoalDetailsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadGoalDetailsAsync()
    {
        // 1. Fetch the master goals using the legal public method
        var goals = await _databaseService.GetGoalsWithAnalyticsAsync();
        ActiveGoal = goals.FirstOrDefault(g => g.Id == GoalId);

        if (ActiveGoal == null) return;

        // 2. Fetch the contributions legally
        var contributions = await _databaseService.GetContributionsForGoalAsync(GoalId);

        // Update the UI list
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ContributionHistory.Clear();
            foreach (var c in contributions)
            {
                ContributionHistory.Add(c);
            }
        });

        decimal remainingAmount = ActiveGoal.TargetAmount - ActiveGoal.CurrentSavings;

        // 3. Calculate Target Pacing
        TimeSpan remainingTime = ActiveGoal.Deadline - DateTime.Today;
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
            DateTime firstDepositDate = contributions.Last().ContributionDate;
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
    private async Task AddDepositAsync()
    {
        if (DepositAmount <= 0 || ActiveGoal == null) return;

        var deposit = new GoalContribution
        {
            GoalId = GoalId,
            Amount = DepositAmount,
            ContributionDate = DateTime.Now
        };

        // Save using the legal public method
        await _databaseService.SaveContributionAsync(deposit);

        // Clear the entry box
        DepositAmount = 0;

        // Recalculate the entire page and refresh the lists instantly
        await LoadGoalDetailsAsync();
    }
}