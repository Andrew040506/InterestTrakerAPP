using SQLite;

namespace InterestTrakerAPP.Models;

public class LedgerAccount
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    // We calculate this dynamically based on transactions, so we don't save it to the DB directly
    [Ignore]
    public decimal CurrentBalance { get; set; }

    [Ignore]
    public string DisplayBalance => $"₱{CurrentBalance:N2}";
}