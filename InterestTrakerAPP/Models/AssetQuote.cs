using System;
using System.Collections.Generic;
using System.Text;

namespace InterestTrakerAPP.Models
{
    public class AssetQuote
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string DisplayPrice => Price > 0 ? $"₱{Price:N2}" : "Loading...";
    }
}
