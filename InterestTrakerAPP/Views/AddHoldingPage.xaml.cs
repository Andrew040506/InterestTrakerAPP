using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class AddHoldingPage : ContentPage
{
    // The constructor uses Dependency Injection to bring in the ViewModel we created
    public AddHoldingPage(AddHoldingViewModel viewModel)
    {
        InitializeComponent();

        // This links the XAML elements to the [ObservableProperty] fields in your ViewModel
        BindingContext = viewModel;
    }
}