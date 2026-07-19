using Microsoft.Maui.Controls;
using System;

namespace InterestTrakerAPP.Views
{
    public partial class LoginPage : ContentPage
    {
        // Track whether the user is logging in or registering
        private bool _isLoginMode = true;

        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnMainActionClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            if (_isLoginMode)
            {
                // TODO: Query your local SQLite database to verify the user

                // For now, bypass the check and launch the app
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                string confirmPassword = ConfirmPasswordEntry.Text;

                if (password != confirmPassword)
                {
                    DisplayAlert("Error", "Passwords do not match.", "OK");
                    return;
                }

                // TODO: Save the new user to your local SQLite database

                // For now, bypass the save and launch the app
                Application.Current.MainPage = new AppShell();
            }
        }

        private void OnToggleModeClicked(object sender, EventArgs e)
        {
            // Flip the mode
            _isLoginMode = !_isLoginMode;

            // Update the UI based on the current mode
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