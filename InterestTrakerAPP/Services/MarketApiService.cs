using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace InterestTrakerAPP.Services
{
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
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}