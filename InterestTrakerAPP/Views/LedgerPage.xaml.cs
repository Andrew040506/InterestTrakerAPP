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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.LoadAccountsCommand.CanExecute(null))
        {
            _viewModel.LoadAccountsCommand.Execute(null);
        }
    }
}