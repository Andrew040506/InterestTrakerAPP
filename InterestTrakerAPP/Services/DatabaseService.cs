using SQLite;
using System;
using System.IO;
using InterestTrakerAPP.Models;
using System.Collections.Generic;

namespace InterestTrakerAPP.Services
{
    public class DatabaseService
    {
        private SQLiteConnection _db;

        public DatabaseService()
        {
            // --- TEMPORARY NUKE CODE ---
            /*
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "InterestTracker.db");
            if (File.Exists(databasePath))
                File.Delete(databasePath);
            */
            // ---------------------------

            Init();
        }

        private void Init()
        {
            if (_db != null) return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "InterestTracker.db");
            _db = new SQLiteConnection(databasePath);

            // Core financial tables
            _db.CreateTable<LedgerAccount>();
            _db.CreateTable<SavingsGoal>();
            _db.CreateTable<FinancialTransaction>();

            _db.CreateTable<AssetQuote>();
            _db.CreateTable<PortfolioItem>();
        }

        public void SeedTestData()
        {
            if (_db.Table<LedgerAccount>().Count() == 0)
            {
                var mainAccount = new LedgerAccount { AccountName = "Main Checking", Balance = 5000.00m, IsActive = true };
                _db.Insert(mainAccount);

                var defenseGoal = new SavingsGoal { Title = "Project Defense Fund", TargetAmount = 1000.00m, CurrentBalance = 0m, TargetDate = DateTime.Now.AddMonths(1) };
                _db.Insert(defenseGoal);

                ExecuteMoneyFlow(mainAccount.Id, null, defenseGoal.Id, 150.50m, "GoalContribution", "Initial seed funding for defense project");
            }
        }

        public void ExecutePortfolioTrade(int accountId, string symbol, string name, string assetClass, decimal units, decimal pricePerUnit, string tradeType, string platform)
        {
            _db.RunInTransaction(() =>
            {
                decimal totalTradeValue = units * pricePerUnit;
                string originAccountName = "Cash on Hand"; // Default for external funds

                // NEW: Set the correct ledger flow! Buying assets is an Outflow of cash. Selling is an Inflow.
                string ledgerFlow = tradeType == "Buy" ? "Outflow" : "Inflow";

                // 1. Process the Cash flow (ONLY if a real ledger account was selected)
                if (accountId != 0)
                {
                    var account = _db.Find<LedgerAccount>(accountId);
                    if (account != null)
                    {
                        if (tradeType == "Buy")
                        {
                            account.Balance -= totalTradeValue; // Money leaves account
                        }
                        else if (tradeType == "Sell")
                        {
                            account.Balance += totalTradeValue; // Money enters account
                        }
                        _db.Update(account);
                        originAccountName = account.AccountName; // Override with actual account name
                    }
                }

                // 2. Process the Asset in the Portfolio
                // CHANGE 1: We now query the database for BOTH the Symbol AND the Platform
                var existingHolding = _db.Table<PortfolioItem>().FirstOrDefault(p => p.Symbol == symbol && p.Platform == platform);

                if (existingHolding != null)
                {
                    if (tradeType == "Buy")
                    {
                        // Calculate new average buy price
                        decimal totalExistingValue = existingHolding.TotalUnits * existingHolding.AverageBuyPrice;
                        decimal newTotalValue = totalExistingValue + totalTradeValue;
                        existingHolding.TotalUnits += units;
                        existingHolding.AverageBuyPrice = newTotalValue / existingHolding.TotalUnits;
                    }
                    else if (tradeType == "Sell")
                    {
                        existingHolding.TotalUnits -= units;
                        // Average buy price stays the same on a sell
                    }
                    _db.Update(existingHolding);
                }
                else if (tradeType == "Buy")
                {
                    // Brand new asset
                    var newHolding = new PortfolioItem
                    {
                        Symbol = symbol,
                        Name = name,
                        AssetClass = assetClass,
                        Platform = platform, // CHANGE 2: We save the new platform string into the database row
                        TotalUnits = units,
                        AverageBuyPrice = pricePerUnit
                    };
                    _db.Insert(newHolding);
                }

                // 3. Log the Immutable Audit Trail
                var auditLog = new FinancialTransaction
                {
                    TransactionType = ledgerFlow,
                    Amount = totalTradeValue,
                    Timestamp = DateTime.UtcNow,
                    Description = $"{tradeType} {units} {symbol} @ ₱{pricePerUnit} on {platform}", // Optional bonus: added platform to the audit log description
                    SourceAccountId = accountId == 0 ? null : accountId,
                    OriginAccount = originAccountName
                };
                _db.Insert(auditLog);
            });
        }
        public void ExecuteMoneyFlow(int sourceAccountId, int? destAccountId, int? targetGoalId, decimal amount, string type, string description)
        {
            _db.RunInTransaction(() =>
            {
                // 1. THE FIX: Use Find() instead of Get(). It returns null instead of crashing!
                var source = _db.Find<LedgerAccount>(sourceAccountId);

                // Only attempt to deduct money if the source account actually exists
                // 1. Check if we are adding or subtracting from the source account
                if (source != null)
                {
                    if (type == "Inflow" || type == "Income")
                    {
                        source.Balance += amount; // Add the funds
                    }
                    else
                    {
                        source.Balance -= amount; // Deduct the funds (Outflow/Expense)
                    }
                    _db.Update(source);
                }

                // 2. Safely handle Destinations using Find()
                if (destAccountId.HasValue)
                {
                    var dest = _db.Find<LedgerAccount>(destAccountId.Value);
                    if (dest != null)
                    {
                        dest.Balance += amount;
                        _db.Update(dest);
                    }
                }
                else if (targetGoalId.HasValue)
                {
                    var goal = _db.Find<SavingsGoal>(targetGoalId.Value);
                    if (goal != null)
                    {
                        goal.CurrentBalance += amount;
                        _db.Update(goal);
                    }
                }

                // 3. Log the Immutable Audit Trail
                var auditLog = new FinancialTransaction
                {
                    TransactionType = type,
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    Description = description,
                    SourceAccountId = sourceAccountId,
                    DestinationAccountId = destAccountId,
                    TargetGoalId = targetGoalId
                };
                _db.Insert(auditLog);
            });
        }

        // --- DATA RETRIEVAL (Zero-Trust Audits) ---
        public List<FinancialTransaction> GetAllTransactions() => _db.Table<FinancialTransaction>().OrderByDescending(t => t.Timestamp).ToList();

        public List<FinancialTransaction> GetLedgerTransactions(int accountId) => _db.Table<FinancialTransaction>().Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId).OrderByDescending(t => t.Timestamp).ToList();

        public List<FinancialTransaction> GetGoalTransactions(int goalId) => _db.Table<FinancialTransaction>().Where(t => t.TargetGoalId == goalId).OrderByDescending(t => t.Timestamp).ToList();

        public List<FinancialTransaction> GetPortfolioTransactions() => _db.Table<FinancialTransaction>().Where(t => t.TransactionType == "PortfolioTrade").OrderByDescending(t => t.Timestamp).ToList();

        // --- CORE ENTITIES ---
        public List<SavingsGoal> GetAllGoals() => _db.Table<SavingsGoal>().ToList();

        public List<LedgerAccount> GetAllAccounts() => _db.Table<LedgerAccount>().ToList();

        public LedgerAccount GetAccount(int accountId) => _db.Table<LedgerAccount>().FirstOrDefault(a => a.Id == accountId);

        // --- ENTITY MANAGEMENT (Mutable Actions) ---
        public void DeleteAccount(LedgerAccount account) => _db.Delete(account);

        public void DeleteGoal(SavingsGoal goal) => _db.Delete(goal);

        public void DeleteWatchlistAsset(object asset) => _db.Delete(asset);

        public void DeleteHolding(object holding) => _db.Delete(holding);

        // --- ENTITY CREATION (Mutable Actions) ---
        // --- ENTITY CREATION & UPDATING (Mutable Actions) ---

        public void SaveAccount(LedgerAccount account)
        {
            if (account.Id != 0) _db.Update(account);
            else _db.Insert(account);
        }

        public void SaveGoal(SavingsGoal goal)
        {
            if (goal.Id != 0) _db.Update(goal);
            else _db.Insert(goal);
        }

        public void SaveHolding(PortfolioItem holding)
        {
            if (holding.Id != 0) _db.Update(holding);
            else _db.Insert(holding);
        }
        public void SaveWatchlistAsset(AssetQuote asset)
        {
            if (asset.Id != 0) _db.Update(asset);
            else _db.Insert(asset);
        }

        // --- ENTITY RETRIEVAL (Mutable Actions) ---
        public List<PortfolioItem> GetAllHoldings() => _db.Table<PortfolioItem>().ToList();
        public List<AssetQuote> GetAllWatchlistAssets() => _db.Table<AssetQuote>().ToList();
    }
}