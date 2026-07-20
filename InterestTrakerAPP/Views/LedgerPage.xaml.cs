using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class LedgerPage : ContentPage
{
    private readonly LedgerViewModel _viewModel;

    public LedgerPage(LedgerViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Aggressively force the page to refresh the total balances and account list
        // every time this screen becomes active, even when navigating backward.
        if (_viewModel != null)
        {
            _viewModel.LoadAccountsCommand.Execute(null);
        }
    }
}