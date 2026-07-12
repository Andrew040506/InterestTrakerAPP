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

        // Ensure BOTH tables are created
        await _database.CreateTableAsync<PortfolioItem>();
        await _database.CreateTableAsync<AssetQuote>();
        await _database.CreateTableAsync<TradeTransaction>();
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
}