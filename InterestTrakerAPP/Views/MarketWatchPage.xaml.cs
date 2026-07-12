using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class MarketWatchPage : ContentPage
{
    public MarketWatchPage(MarketWatchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Unfocus the search bar when the ViewModel clears the text (after adding)
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MarketWatchViewModel.SearchSymbolText) &&
                string.IsNullOrEmpty(viewModel.SearchSymbolText))
            {
                AssetSearchBar.Unfocus();
            }
        };
    }
}