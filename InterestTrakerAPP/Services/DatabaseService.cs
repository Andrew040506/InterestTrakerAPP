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
}