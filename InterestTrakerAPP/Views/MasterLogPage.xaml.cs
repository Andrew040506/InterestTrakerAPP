using Microsoft.Maui.Controls;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.Views
{
    public partial class MasterLogPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public MasterLogPage(DatabaseService dbService)
        {
            InitializeComponent();

            // Store the injected, unlocked singleton
            _dbService = dbService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh the UI with the latest data every time the page comes into view
            LoadAuditTrail();
        }

        private void LoadAuditTrail()
        {
            // Use the enriched display items so account names are visible
            var allTransactions = _dbService.GetAllTransactionDisplayItems();

            // Bind the data directly to the XAML CollectionView
            TransactionsListView.ItemsSource = allTransactions;
        }
    }
}