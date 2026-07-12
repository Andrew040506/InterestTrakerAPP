using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class PortfolioPage : ContentPage
{
    public PortfolioPage(PortfolioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}