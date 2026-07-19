using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.ViewModels;

public partial class GoalsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private bool _isAddingGoal; // Toggles the creation form

    // Form Inputs
    [ObservableProperty] private string _newGoalTitle = string.Empty;
    [ObservableProperty] private decimal _newGoalTargetAmount;
    [ObservableProperty] private DateTime _newGoalDeadline = DateTime.Now.AddMonths(3);

    public ObservableCollection<SavingsGoal> Goals { get; } = new();

    public GoalsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadGoalsAsync()
    {
        IsRefreshing = true;
        var goals = await _databaseService.GetGoalsWithAnalyticsAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Goals.Clear();
            foreach (var goal in goals)
            {
                Goals.Add(goal);
            }
            IsRefreshing = false;
        });
    }

    [RelayCommand]
    private void ToggleAddGoalForm()
    {
        IsAddingGoal = !IsAddingGoal;
    }

    [RelayCommand]
    private async Task SaveNewGoalAsync()
    {
        if (string.IsNullOrWhiteSpace(NewGoalTitle) || NewGoalTargetAmount <= 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a valid title and target amount.", "OK");
            return;
        }

        var newGoal = new SavingsGoal
        {
            Title = NewGoalTitle,
            TargetAmount = NewGoalTargetAmount,
            Deadline = NewGoalDeadline,
            CreatedAt = DateTime.Now
        };

        await _databaseService.SaveGoalAsync(newGoal);

        // Reset form and hide it
        NewGoalTitle = string.Empty;
        NewGoalTargetAmount = 0;
        NewGoalDeadline = DateTime.Now.AddMonths(3);
        IsAddingGoal = false;

        await LoadGoalsAsync();
    }

    [RelayCommand]
    private async Task DeleteGoalAsync(SavingsGoal goal)
    {
        if (goal == null) return;

        bool confirm = await Shell.Current.DisplayAlert("Delete Goal", $"Erase '{goal.Title}' and all its tracking data?", "Delete", "Cancel");
        if (confirm)
        {
            await _databaseService.DeleteGoalAsync(goal);
            await LoadGoalsAsync();
        }
    }

    [RelayCommand]
    private async Task NavigateToDetailsAsync(SavingsGoal goal)
    {
        if (goal == null) return;

        var navParams = new Dictionary<string, object>
        {
            { "GoalId", goal.Id }
        };

        await Shell.Current.GoToAsync("GoalDetailsPage", navParams);
    }
}