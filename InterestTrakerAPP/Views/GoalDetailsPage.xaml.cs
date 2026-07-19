using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class GoalDetailsPage : ContentPage
{
    private readonly GoalDetailsViewModel _viewModel;

    public GoalDetailsPage(GoalDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Fire the forecasting algorithms the moment the page appears
        if (_viewModel.LoadGoalDetailsCommand.CanExecute(null))
        {
            _viewModel.LoadGoalDetailsCommand.Execute(null);
        }
    }
}