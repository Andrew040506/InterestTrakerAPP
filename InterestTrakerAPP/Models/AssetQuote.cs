using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace InterestTrakerAPP.Models;

public partial class AssetQuote : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Symbol { get; set; } = string.Empty;
    public string AssetClass { get; set; } = string.Empty;

    // The [property: Ignore] tag tells SQLite not to save live API prices to your hard drive
    [ObservableProperty]
    [property: Ignore]
    [NotifyPropertyChangedFor(nameof(DisplayPriceUsd))]
    [NotifyPropertyChangedFor(nameof(DisplayPricePhp))]
    private decimal _priceUsd;

    [ObservableProperty]
    [property: Ignore]
    [NotifyPropertyChangedFor(nameof(DisplayPricePhp))]
    private decimal _currentPhpRate;

    [Ignore] public string DisplayPriceUsd => PriceUsd > 0 ? $"${PriceUsd:N2}" : "Loading...";
    [Ignore] public string DisplayPricePhp => PriceUsd > 0 ? $"₱{(PriceUsd * CurrentPhpRate):N2}" : "";
}