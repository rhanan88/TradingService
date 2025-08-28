using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingService.Core;
using TradingService.Core.DTOs;
using TradingService.Core.Interfaces;
using TradingService.Core.Messages;

namespace TradingService.Infrastructure
{
    public class TradeService : ITradeService
    {
        private readonly TradingDbContext _context;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<TradeService> _logger;

        public TradeService(TradingDbContext context, IMessagePublisher messagePublisher, ILogger<TradeService> logger)
        {
            _context = context;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        public async Task<TradeResponseDto> ExecuteTradeAsync(CreateTradeDto tradeDto)
        {
            _logger.LogInformation("Executing trade for user {UserId}: {Quantity} {Symbol} at {Price}",
                tradeDto.UserId, tradeDto.Quantity, tradeDto.Symbol, tradeDto.Price);

            var trade = new Trade
            {
                Symbol = tradeDto.Symbol,
                Quantity = tradeDto.Quantity,
                Price = tradeDto.Price,
                Type = tradeDto.Type,
                UserId = tradeDto.UserId,
                Status = TradeStatus.Executed
            };

            _context.Trades.Add(trade);
            await _context.SaveChangesAsync();

            // Publish message to queue
            var message = new TradeExecutedMessage
            {
                TradeId = trade.Id,
                Symbol = trade.Symbol,
                Quantity = trade.Quantity,
                Price = trade.Price,
                Type = trade.Type,
                UserId = trade.UserId,
                ExecutedAt = trade.ExecutedAt,
                TotalValue = trade.TotalValue
            };

            await _messagePublisher.PublishAsync(message, "trade.executed");

            _logger.LogInformation("Trade {TradeId} executed successfully", trade.Id);

            return MapToDto(trade);
        }

        public async Task<IEnumerable<TradeResponseDto>> GetTradesAsync(string? userId = null)
        {
            var query = _context.Trades.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(t => t.UserId == userId);
            }

            var trades = await query
                .OrderByDescending(t => t.ExecutedAt)
                .ToListAsync();

            return trades.Select(MapToDto);
        }

        public async Task<TradeResponseDto?> GetTradeByIdAsync(int id)
        {
            var trade = await _context.Trades.FindAsync(id);
            return trade != null ? MapToDto(trade) : null;
        }

        private static TradeResponseDto MapToDto(Trade trade)
        {
            return new TradeResponseDto
            {
                Id = trade.Id,
                Symbol = trade.Symbol,
                Quantity = trade.Quantity,
                Price = trade.Price,
                Type = trade.Type,
                UserId = trade.UserId,
                ExecutedAt = trade.ExecutedAt,
                Status = trade.Status,
                TotalValue = trade.TotalValue
            };
        }
    }
}
