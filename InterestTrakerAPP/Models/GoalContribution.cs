using SQLite;

namespace InterestTrakerAPP.Models;

public class GoalContribution
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int GoalId { get; set; }

    public decimal Amount { get; set; }
    public DateTime ContributionDate { get; set; } = DateTime.Now;
}