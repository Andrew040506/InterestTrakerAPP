using System.Text.Json;

namespace InterestTrakerAPP.Services;

public class MarketApiService
{
    private readonly HttpClient _httpClient;
    private const string API_KEY = "d99ik9hr01qssj13do7gd99ik9hr01qssj13do80";

    public MarketApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://finnhub.io/api/v1/")
        };
    }

    public async Task<decimal?> GetLivePriceAsync(string symbol)
    {
        try
        {
            var response = await _httpClient.GetAsync($"quote?symbol={symbol}&token={API_KEY}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("c").GetDecimal();
        }
        catch
        {
            return null;
        }
    }

    // NEW: Fetches the live PHP conversion rate
    public async Task<decimal> GetUsdToPhpRateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            // Extracts the specific PHP multiplier from the JSON payload
            return document.RootElement.GetProperty("rates").GetProperty("PHP").GetDecimal();
        }
        catch
        {
            return 58.50m; // A safe fallback if you lose internet connection
        }
    }
}