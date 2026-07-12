using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class MarketWatchPage : ContentPage
{
    public MarketWatchPage(MarketWatchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}