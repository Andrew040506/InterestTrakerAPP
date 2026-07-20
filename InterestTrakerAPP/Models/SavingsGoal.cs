using SQLite;
using System;

namespace InterestTrakerAPP.Models
{
    public class SavingsGoal
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime TargetDate { get; set; }

        // Added this so the XAML ProgressBar has a value to bind to
        [Ignore]
        public double ProgressPercentage => TargetAmount > 0 ? (double)(CurrentBalance / TargetAmount) : 0;
    }
}