using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingService.Core
{
    public class Trade
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public TradeType Type { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public TradeStatus Status { get; set; } = TradeStatus.Pending;
        public decimal TotalValue => Quantity * Price;
    }

    public enum TradeType
    {
        Buy = 1,
        Sell = 2
    }

    public enum TradeStatus
    {
        Pending = 1,
        Executed = 2,
        Failed = 3
    }
}
