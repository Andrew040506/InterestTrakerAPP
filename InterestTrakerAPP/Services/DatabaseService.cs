using SQLite;
using InterestTrakerAPP.Models;

namespace InterestTrakerAPP.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database = null!;

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "WealthShield.db3");
        _database = new SQLiteAsyncConnection(databasePath);

        await _database.CreateTableAsync<PortfolioItem>();
        await _database.CreateTableAsync<AssetQuote>();
        await _database.CreateTableAsync<TradeTransaction>();

        await _database.CreateTableAsync<LedgerAccount>();
        await _database.CreateTableAsync<LedgerTransaction>();

        await _database.CreateTableAsync<SavingsGoal>();
        await _database.CreateTableAsync<GoalContribution>();
    }

    // --- PORTFOLIO METHODS ---
    public async Task<List<PortfolioItem>> GetHoldingsAsync()
    {
        await InitAsync();
        return await _database.Table<PortfolioItem>().ToListAsync();
    }

    public async Task<int> SaveHoldingAsync(PortfolioItem item)
    {
        await InitAsync();
        if (item.Id != 0)
        {
            return await _database.UpdateAsync(item);
        }
        else
        {
            return await _database.InsertAsync(item);
        }
    }

    public async Task<int> DeleteHoldingAsync(PortfolioItem item)
    {
        await InitAsync();
        return await _database.DeleteAsync(item);
    }

    // --- NEW: WATCHLIST METHODS ---
    public async Task<List<AssetQuote>> GetWatchlistAsync()
    {
        await InitAsync();
        return await _database.Table<AssetQuote>().ToListAsync();
    }

    public async Task<int> SaveWatchlistAssetAsync(AssetQuote item)
    {
        await InitAsync();
        if (item.Id != 0)
            return await _database.UpdateAsync(item);
        else
            return await _database.InsertAsync(item);
    }

    public async Task<int> DeleteWatchlistAssetAsync(AssetQuote item)
    {
        await InitAsync();
        return await _database.DeleteAsync(item);
    }

    // --- NEW: TRANSACTION LEDGER METHODS ---

    // Gets all trades, or filters by a specific symbol if you pass one in
    public async Task<List<TradeTransaction>> GetTransactionsAsync(string symbol = "")
    {
        await InitAsync();

        if (string.IsNullOrEmpty(symbol))
        {
            // Return all trades, newest first
            return await _database.Table<TradeTransaction>()
                                  .OrderByDescending(t => t.TradeDate)
                                  .ToListAsync();
        }

        // Return trades for a specific asset
        return await _database.Table<TradeTransaction>()
                              .Where(t => t.Symbol == symbol)
                              .OrderByDescending(t => t.TradeDate)
                              .ToListAsync();
    }

    public async Task<int> SaveTransactionAsync(TradeTransaction transaction)
    {
        await InitAsync();
        if (transaction.Id != 0)
            return await _database.UpdateAsync(transaction);
        else
            return await _database.InsertAsync(transaction);
    }

    public async Task<int> DeleteTransactionAsync(TradeTransaction transaction)
    {
        await InitAsync();
        return await _database.DeleteAsync(transaction);
    }

    // --- NEW: LEDGER & CASH METHODS ---

    public async Task<List<LedgerAccount>> GetAccountsAsync()
    {
        await InitAsync();
        var accounts = await _database.Table<LedgerAccount>().ToListAsync();

        // Calculate the live balance for each account
        foreach (var acc in accounts)
        {
            var transactions = await _database.Table<LedgerTransaction>()
                                              .Where(t => t.AccountId == acc.Id)
                                              .ToListAsync();

            decimal inflows = transactions.Where(t => t.Type == "Inflow").Sum(t => t.Amount);
            decimal outflows = transactions.Where(t => t.Type == "Outflow").Sum(t => t.Amount);
            acc.CurrentBalance = inflows - outflows;
        }
        return accounts;
    }

    public async Task<int> SaveAccountAsync(LedgerAccount account)
    {
        await InitAsync();
        if (account.Id != 0) return await _database.UpdateAsync(account);
        else return await _database.InsertAsync(account);
    }

    public async Task<int> DeleteAccountAsync(LedgerAccount account)
    {
        await InitAsync();
        // Delete the account AND all its connected transactions to prevent orphaned data
        var transactions = await _database.Table<LedgerTransaction>().Where(t => t.AccountId == account.Id).ToListAsync();
        foreach (var t in transactions) { await _database.DeleteAsync(t); }

        return await _database.DeleteAsync(account);
    }
    public async Task<List<LedgerTransaction>> GetLedgerTransactionsAsync(int accountId)
    {
        await InitAsync();
        return await _database.Table<LedgerTransaction>()
                              .Where(t => t.AccountId == accountId)
                              .OrderByDescending(t => t.Timestamp)
                              .ToListAsync();
    }

    public async Task<int> SaveLedgerTransactionAsync(LedgerTransaction transaction)
    {
        await InitAsync();
        return await _database.InsertAsync(transaction);
    }

    public async Task<int> DeleteLedgerTransactionAsync(LedgerTransaction transaction)
    {
        await InitAsync();
        return await _database.DeleteAsync(transaction);
    }

    public async Task<List<SavingsGoal>> GetGoalsWithAnalyticsAsync()
    {
        await InitAsync();
        var goals = await _database.Table<SavingsGoal>().ToListAsync();

        foreach (var goal in goals)
        {
            var contributions = await _database.Table<GoalContribution>()
                                                .Where(c => c.GoalId == goal.Id)
                                                .OrderBy(c => c.ContributionDate)
                                                .ToListAsync();

            goal.CurrentSavings = contributions.Sum(c => c.Amount);
        }

        return goals;
    }

    public async Task<int> SaveGoalAsync(SavingsGoal goal)
    {
        await InitAsync();
        if (goal.Id != 0) return await _database.UpdateAsync(goal);
        else return await _database.InsertAsync(goal);
    }

    public async Task<int> SaveContributionAsync(GoalContribution contribution)
    {
        await InitAsync();
        return await _database.InsertAsync(contribution);
    }

    public async Task<List<GoalContribution>> GetContributionsForGoalAsync(int goalId)
    {
        await InitAsync();
        return await _database.Table<GoalContribution>()
                              .Where(c => c.GoalId == goalId)
                              .OrderByDescending(c => c.ContributionDate)
                              .ToListAsync();
    }

    public async Task<int> DeleteGoalAsync(SavingsGoal goal)
    {
        await InitAsync();

        var contributions = await _database.Table<GoalContribution>().Where(c => c.GoalId == goal.Id).ToListAsync();
        foreach (var c in contributions)
        {
            await _database.DeleteAsync(c);
        }

        return await _database.DeleteAsync(goal);
    }

}