using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingService.Core.DTOs
{
    public class CreateTradeDto
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public TradeType Type { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class TradeResponseDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public TradeType Type { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public TradeStatus Status { get; set; }
        public decimal TotalValue { get; set; }
    }
}
