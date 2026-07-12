using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class PortfolioPage : ContentPage
{
    public PortfolioPage(PortfolioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddHoldingPage));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // This forces the ViewModel to reload the database every time you return to the screen
        if (BindingContext is ViewModels.PortfolioViewModel vm)
        {
            _ = vm.LoadPortfolioDataCommand.ExecuteAsync(null);
        }
    }

}