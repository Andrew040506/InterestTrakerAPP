using Microsoft.Maui.Controls;
using System;
using InterestTrakerAPP.Services;

namespace InterestTrakerAPP.Views
{
    public partial class LoginPage : ContentPage
    {
        private bool _isLoginMode = true;
        private readonly AuthService _authService;
        private readonly DatabaseService _databaseService;

        // Inject the services through the constructor
        public LoginPage(AuthService authService, DatabaseService databaseService)
        {
            InitializeComponent();
            _authService = authService;
            _databaseService = databaseService;
        }

        private async void OnMainActionClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text?.Trim();
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (_isLoginMode)
            {
                // 1. Check Auth.db
                if (_authService.Login(username, password))
                {
                    // 2. Success! Point the app to this specific user's file
                    _databaseService.InitializeForUser(username);

                    // 3. Let them into the vault
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await DisplayAlert("Access Denied", "Invalid username or password.", "OK");
                }
            }
            else
            {
                string confirmPassword = ConfirmPasswordEntry.Text;

                if (password != confirmPassword)
                {
                    await DisplayAlert("Error", "Passwords do not match.", "OK");
                    return;
                }

                // 1. Try to save to Auth.db
                if (_authService.Register(username, password))
                {
                    // 2. Success! Immediately log them in and build their isolated file
                    _databaseService.InitializeForUser(username);
                    await DisplayAlert("Success", "Account created and secured.", "OK");

                    // 3. Let them into the vault
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await DisplayAlert("Error", "Username already exists.", "OK");
                }
            }
        }

        private void OnToggleModeClicked(object sender, EventArgs e)
        {
            // ... (keep your existing toggle logic exactly as is)
            _isLoginMode = !_isLoginMode;

            if (_isLoginMode)
            {
                HeaderLabel.Text = "Welcome Back";
                SubHeaderLabel.Text = "Log in to access your ledger";
                ConfirmPasswordBorder.IsVisible = false;
                MainActionButton.Text = "Login";
                ToggleModeButton.Text = "Don't have an account? Create one";
            }
            else
            {
                HeaderLabel.Text = "Create Account";
                SubHeaderLabel.Text = "Join the zero-trust ecosystem";
                ConfirmPasswordBorder.IsVisible = true;
                MainActionButton.Text = "Register";
                ToggleModeButton.Text = "Already have an account? Log in";
            }
        }
    }
}