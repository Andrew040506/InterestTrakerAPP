using SQLite;

namespace InterestTrakerAPP.Models;

public class LedgerTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed] // Links back to the LedgerAccount
    public int AccountId { get; set; }

    public string Type { get; set; } = "Inflow"; // "Inflow" or "Outflow"

    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Ignore]
    public string DisplayAmount => Type == "Inflow" ? $"+₱{Amount:N2}" : $"-₱{Amount:N2}";
}