using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingService.Core.DTOs;

namespace TradingService.Core.Interfaces
{
    public interface ITradeService
    {
        Task<TradeResponseDto> ExecuteTradeAsync(CreateTradeDto tradeDto);
        Task<IEnumerable<TradeResponseDto>> GetTradesAsync(string? userId = null);
        Task<TradeResponseDto?> GetTradeByIdAsync(int id);
    }
}
