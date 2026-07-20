using Microsoft.Maui.Controls;
using InterestTrakerAPP.Views;

namespace InterestTrakerAPP
{
    public partial class App : Application
    {
        // We inject the LoginPage directly into the App constructor
        public App(LoginPage loginPage)
        {
            InitializeComponent();

            // Let MAUI assign the fully constructed page (with all its services) to the MainPage
            MainPage = loginPage;
        }
    }
}