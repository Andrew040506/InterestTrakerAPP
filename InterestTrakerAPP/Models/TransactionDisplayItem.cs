using System;

namespace InterestTrakerAPP.Models
{
    /// <summary>
    /// A display-layer wrapper that enriches a raw FinancialTransaction
    /// with resolved human-readable account/goal names for UI presentation.
    /// </summary>
    public class TransactionDisplayItem
    {
        // --- Raw transaction passthrough ---
        public int Id { get; init; }
        public string TransactionType { get; init; }
        public decimal Amount { get; init; }
        public DateTime Timestamp { get; init; }
        public string Description { get; init; }

        // --- Resolved names (set at query-time by DatabaseService) ---
        public string SourceAccountName { get; init; }
        public string DestinationAccountName { get; init; }
        public string TargetGoalTitle { get; init; }

        /// <summary>
        /// A rich label like "Inflow → Main Checking" or "Outflow ← BDO Savings"
        /// used in the Master Log and Account Details transaction lists.
        /// </summary>
        public string DisplayType
        {
            get
            {
                return TransactionType switch
                {
                    "Inflow" when !string.IsNullOrEmpty(SourceAccountName)
                        => $"Inflow → {SourceAccountName}",

                    "Outflow" when !string.IsNullOrEmpty(SourceAccountName)
                        => $"Outflow ← {SourceAccountName}",

                    "GoalContribution" when !string.IsNullOrEmpty(TargetGoalTitle) &&
                                           !string.IsNullOrEmpty(SourceAccountName)
                        => $"Goal Savings: {TargetGoalTitle} ← {SourceAccountName}",

                    "GoalContribution" when !string.IsNullOrEmpty(TargetGoalTitle)
                        => $"Goal Savings: {TargetGoalTitle}",

                    // Portfolio trades — show destination account if proceeds go somewhere
                    "Inflow" when !string.IsNullOrEmpty(DestinationAccountName)
                        => $"Inflow → {DestinationAccountName}",

                    "Outflow" when !string.IsNullOrEmpty(DestinationAccountName)
                        => $"Outflow → {DestinationAccountName}",

                    _ => TransactionType
                };
            }
        }

        /// <summary>
        /// Color hint: green for inflows, red for outflows, purple for trades.
        /// Used by DataTriggers / converters in XAML.
        /// </summary>
        public bool IsPositive => TransactionType == "Inflow" || TransactionType == "Sell";
    }
}
