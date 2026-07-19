using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class GoalsPage : ContentPage
{
    private readonly GoalsViewModel _viewModel;

    public GoalsPage(GoalsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadGoalsCommand.CanExecute(null))
        {
            _viewModel.LoadGoalsCommand.Execute(null);
        }
    }
}