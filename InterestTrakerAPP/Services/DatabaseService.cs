using SQLite;
using InterestTrakerAPP.Models;

namespace InterestTrakerAPP.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        // Creates a secure local file on your phone/computer
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "WealthShield.db3");

        _database = new SQLiteAsyncConnection(databasePath);

        // Builds the table based on your PortfolioItem properties
        await _database.CreateTableAsync<PortfolioItem>();
    }

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
}