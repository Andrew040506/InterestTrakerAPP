using CommunityToolkit.Mvvm.ComponentModel;
using SQLite; // ADDED: SQLite namespace

namespace InterestTrakerAPP.Models;

public partial class PortfolioItem : ObservableObject
{
    // ADDED: The unique ID for the database
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssetClass { get; set; } = string.Empty;
    public decimal TotalUnits { get; set; }
    public decimal AverageBuyPrice { get; set; }

    // ADDED: [Ignore] tells SQLite NOT to save these live API values to the local file
    [ObservableProperty]
    [property: Ignore]
    [NotifyPropertyChangedFor(nameof(TotalValue))]
    [NotifyPropertyChangedFor(nameof(UnrealizedPnL))]
    [NotifyPropertyChangedFor(nameof(PnLPercentage))]
    [NotifyPropertyChangedFor(nameof(DisplayTotalValue))]
    [NotifyPropertyChangedFor(nameof(DisplayLivePrice))]
    [NotifyPropertyChangedFor(nameof(DisplayPnL))]
    [NotifyPropertyChangedFor(nameof(IsProfitable))]
    private decimal _livePrice;

    // --- Math Calculations ---
    [Ignore] public decimal TotalValue => TotalUnits * LivePrice;
    [Ignore] public decimal UnrealizedPnL => (LivePrice - AverageBuyPrice) * TotalUnits;
    [Ignore] public decimal PnLPercentage => AverageBuyPrice > 0 ? ((LivePrice - AverageBuyPrice) / AverageBuyPrice) * 100 : 0;
    [Ignore] public bool IsProfitable => UnrealizedPnL >= 0;

    // --- Formatted Strings for XAML ---
    [Ignore] public string DisplayTotalValue => $"₱{TotalValue:N2}";
    [Ignore] public string DisplayLivePrice => $"₱{LivePrice:N2}";
    [Ignore] public string DisplayHoldings => $"{TotalUnits:N4} units";
    [Ignore] public string DisplayPnL => $"{(IsProfitable ? "+" : "")}₱{UnrealizedPnL:N2} ({PnLPercentage:N1}%)";
    [Ignore] public string AvgCostDisplay => $"Avg: ₱{AverageBuyPrice:N2}";
    [Ignore] public string IconLetter => string.IsNullOrEmpty(Symbol) ? "" : Symbol.Substring(0, 1).ToUpper();
}