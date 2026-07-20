using System;
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
    public partial class GoalsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty] private bool _isRefreshing;
        [ObservableProperty] private bool _isAddingGoal;

        // Form Inputs
        [ObservableProperty] private string _newGoalTitle = string.Empty;
        [ObservableProperty] private decimal _newGoalTargetAmount;
        [ObservableProperty] private DateTime _newGoalTargetDate = DateTime.Now.AddMonths(3);

        public ObservableCollection<SavingsGoal> Goals { get; } = new();

        public GoalsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public void LoadGoals()
        {
            IsRefreshing = true;
            var goals = _databaseService.GetAllGoals();

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
                TargetDate = NewGoalTargetDate, // Updated property
                CurrentBalance = 0
            };

            _databaseService.SaveGoal(newGoal);

            NewGoalTitle = string.Empty;
            NewGoalTargetAmount = 0;
            NewGoalTargetDate = DateTime.Now.AddMonths(3);
            IsAddingGoal = false;

            LoadGoals();
        }

        [RelayCommand]
        private async Task DeleteGoalAsync(SavingsGoal goal)
        {
            if (goal == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete Goal", $"Erase '{goal.Title}' and all its tracking data?", "Delete", "Cancel");
            if (confirm)
            {
                _databaseService.DeleteGoal(goal);
                LoadGoals();
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
}