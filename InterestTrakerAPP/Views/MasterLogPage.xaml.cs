using Microsoft.Maui.Controls;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.Views
{
    public partial class MasterLogPage : ContentPage
    {
        private DatabaseService _dbService;

        public MasterLogPage()
        {
            InitializeComponent();

            // Initialize the database connection
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh the UI with the latest data every time the page comes into view
            LoadAuditTrail();
        }

        private void LoadAuditTrail()
        {
            // Call the master filter method we just built in the DatabaseService
            var allTransactions = _dbService.GetAllTransactions();

            // Bind the data directly to the XAML CollectionView
            TransactionsListView.ItemsSource = allTransactions;
        }
    }
}