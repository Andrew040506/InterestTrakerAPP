using SQLite;

namespace InterestTrakerAPP.Models;

public class TradeTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string TradeType { get; set; } = "Buy";
    public decimal Units { get; set; }
    public decimal PricePerUnit { get; set; }
    public DateTime TradeDate { get; set; }

    [Ignore]
    public decimal TotalTradeValue => Units * PricePerUnit;
}