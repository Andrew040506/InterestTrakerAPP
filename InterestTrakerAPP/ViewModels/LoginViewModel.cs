using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InterestTrakerAPP.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private string _promptText = "Loading Secure Enclave...";
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isSetupMode;

    public async Task CheckSetupStatusAsync()
    {
        // Check the device's encrypted storage for an existing PIN
        var existingPin = await SecureStorage.Default.GetAsync("AppSecurityPin");

        if (string.IsNullOrEmpty(existingPin))
        {
            IsSetupMode = true;
            PromptText = "Create a 4-Digit Security PIN";
        }
        else
        {
            IsSetupMode = false;
            PromptText = "Enter your Security PIN";
        }
    }

    [RelayCommand]
    private async Task SubmitPinAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Pin) || Pin.Length < 4)
        {
            ErrorMessage = "PIN must be at least 4 digits.";
            return;
        }

        if (IsSetupMode)
        {
            // Save the new PIN securely
            await SecureStorage.Default.SetAsync("AppSecurityPin", Pin);
            UnlockApp();
        }
        else
        {
            // Verify existing PIN
            var savedPin = await SecureStorage.Default.GetAsync("AppSecurityPin");
            if (Pin == savedPin)
            {
                UnlockApp();
            }
            else
            {
                ErrorMessage = "Incorrect PIN. Try again.";
                Pin = string.Empty; // Clear the field for retry
            }
        }
    }

    private void UnlockApp()
    {
        // This is the magic! We completely swap out the Login Page for your App's Tabs.
        // This ensures the user cannot press the hardware "Back" button to escape.
        Application.Current.MainPage = new AppShell();
    }
}