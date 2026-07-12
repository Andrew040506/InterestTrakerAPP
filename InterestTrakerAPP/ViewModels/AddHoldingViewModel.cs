using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InterestTrakerAPP.Models;
using InterestTrakerAPP.Services;


namespace InterestTrakerAPP.ViewModels;

public partial class AddHoldingViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty] private string _symbol = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _assetClass = "Stocks"; // Default
    [ObservableProperty] private decimal _units;
    [ObservableProperty] private decimal _averageBuyPrice;

    public AddHoldingViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    private async Task SaveHoldingAsync()
    {
        if (string.IsNullOrWhiteSpace(Symbol) || Units <= 0) return;

        var newItem = new PortfolioItem
        {
            Symbol = Symbol.ToUpper(),
            Name = Name,
            AssetClass = AssetClass,
            TotalUnits = Units,
            AverageBuyPrice = AverageBuyPrice
        };

        await _databaseService.SaveHoldingAsync(newItem);

        // Return to the previous page
        await Shell.Current.GoToAsync("..");
    }
}