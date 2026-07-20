using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class GoalDetailsPage : ContentPage
{
    public GoalDetailsPage(GoalDetailsViewModel viewModel)
    {
        InitializeComponent();

        // This links the XAML to your new ViewModel properties
        BindingContext = viewModel;
    }
}