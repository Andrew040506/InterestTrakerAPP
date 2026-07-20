using SQLite;
using System;

namespace InterestTrakerAPP.Models
{
    public class FinancialTransaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string TransactionType { get; set; }
        public string OriginAccount { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }

        public int? SourceAccountId { get; set; }
        public int? DestinationAccountId { get; set; }
        public int? TargetGoalId { get; set; }
    }
}