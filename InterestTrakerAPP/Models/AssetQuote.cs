namespace InterestTrakerAPP.Models;

public class AssetQuote
{
    public string Symbol { get; set; } = string.Empty;
    public string AssetClass { get; set; } = string.Empty; // "Crypto" or "Stocks"
    public decimal PriceUsd { get; set; }
    public decimal CurrentPhpRate { get; set; } // The live conversion rate
    public string DisplayPriceUsd => PriceUsd > 0 ? $"${PriceUsd:N2}" : "Loading...";
    public string DisplayPricePhp => PriceUsd > 0 ? $"₱{(PriceUsd * CurrentPhpRate):N2}" : "";
}