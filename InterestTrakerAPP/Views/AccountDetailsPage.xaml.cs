using InterestTrakerAPP.ViewModels;

namespace InterestTrakerAPP.Views;

public partial class AccountDetailsPage : ContentPage
{
    private readonly AccountDetailsViewModel _viewModel;

    public AccountDetailsPage(AccountDetailsViewModel viewModel)
    {
        InitializeComponent();

        // Wire up the injected view model as the runtime data binding source
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // We bypass the "CanExecute" check here. We are aggressively 
        // demanding that the UI refreshes no matter what.
        if (_viewModel != null)
        {
            _viewModel.LoadDataCommand.Execute(null);
        }
    }
}