using SQLite;

namespace InterestTrakerAPP.Models;

public class SavingsGoal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Ignore]
    public decimal CurrentSavings { get; set; }

    [Ignore]
    public double ProgressPercentage => TargetAmount > 0 ? (double)(CurrentSavings / TargetAmount) : 0;
}