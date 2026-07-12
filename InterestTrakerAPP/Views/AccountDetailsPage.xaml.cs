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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Automatically fire the retrieval command when the user opens the page.
        // This forces SQLite to refresh records and update the active balance text blocks.
        if (_viewModel.LoadDataCommand.CanExecute(null))
        {
            _viewModel.LoadDataCommand.Execute(null);
        }
    }
}