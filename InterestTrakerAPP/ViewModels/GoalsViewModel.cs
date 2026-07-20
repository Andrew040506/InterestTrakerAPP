using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;
using InterestTrakerAPP.Views;

namespace InterestTrakerAPP.ViewModels
{
    public partial class GoalsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<SavingsGoal> _goals = new();

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _isAddingGoal;

        [ObservableProperty]
        private string _newGoalTitle;

        [ObservableProperty]
        private decimal _newGoalTargetAmount;

        [ObservableProperty]
        private DateTime _newGoalDeadline = DateTime.Today.AddMonths(1);

        public GoalsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoadGoals();
        }

        [RelayCommand]
        private void LoadGoals()
        {
            IsRefreshing = true;

            var list = _databaseService.GetAllGoals();
            Goals.Clear();
            foreach (var goal in list)
            {
                Goals.Add(goal);
            }

            IsRefreshing = false;
        }

        [RelayCommand]
        private void ToggleAddGoalForm()
        {
            IsAddingGoal = !IsAddingGoal;
        }

        [RelayCommand]
        private void SaveNewGoal()
        {
            if (string.IsNullOrWhiteSpace(NewGoalTitle) || NewGoalTargetAmount <= 0)
                return;

            // Fixed: Use SaveGoal (or your database service's correct insert method)
            _databaseService.SaveGoal(new SavingsGoal
            {
                Title = NewGoalTitle,
                TargetAmount = NewGoalTargetAmount,
                TargetDate = NewGoalDeadline,
                CurrentBalance = 0
            });

            NewGoalTitle = string.Empty;
            NewGoalTargetAmount = 0;
            IsAddingGoal = false;

            LoadGoals();
        }

        [RelayCommand]
        private void DeleteGoal(SavingsGoal goal)
        {
            if (goal != null)
            {
                // Fixed: Pass the 'goal' object directly if the method expects the entity model
                _databaseService.DeleteGoal(goal);
                Goals.Remove(goal);
            }
        }

        [RelayCommand]
        private async Task NavigateToDetails(SavingsGoal selectedGoal)
        {
            if (selectedGoal == null) return;

            // Fixed: Use Shell QueryParameters to pass the ID automatically to [QueryProperty]
            await Shell.Current.GoToAsync($"{nameof(GoalDetailsPage)}?GoalId={selectedGoal.Id}");
        }
    }
}