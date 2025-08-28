using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TradingService.Core;
using TradingService.Core.DTOs;
using TradingService.Core.Interfaces;
using TradingService.Infrastructure;
using Xunit;

namespace TradingService.Tests
{
    public class TradeServiceTests : IDisposable
    {
        private readonly TradingDbContext _context;
        private readonly Mock<IMessagePublisher> _mockPublisher;
        private readonly Mock<ILogger<TradeService>> _mockLogger;
        private readonly TradeService _tradeService;

        public TradeServiceTests()
        {
            var options = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TradingDbContext(options);
            _mockPublisher = new Mock<IMessagePublisher>();
            _mockLogger = new Mock<ILogger<TradeService>>();
            _tradeService = new TradeService(_context, _mockPublisher.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ExecuteTradeAsync_ShouldCreateTradeAndPublishMessage()
        {
            // Arrange
            var tradeDto = new CreateTradeDto
            {
                Symbol = "AAPL",
                Quantity = 100,
                Price = 150.50m,
                Type = TradeType.Buy,
                UserId = "user123"
            };

            // Act
            var result = await _tradeService.ExecuteTradeAsync(tradeDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("AAPL", result.Symbol);
            Assert.Equal(100, result.Quantity);
            Assert.Equal(150.50m, result.Price);
            Assert.Equal(TradeType.Buy, result.Type);
            Assert.Equal("user123", result.UserId);
            Assert.Equal(TradeStatus.Executed, result.Status);
            Assert.Equal(15050m, result.TotalValue);

            var tradeInDb = await _context.Trades.FindAsync(result.Id);
            Assert.NotNull(tradeInDb);

            _mockPublisher.Verify(p => p.PublishAsync(It.IsAny<object>(), "trade.executed"), Times.Once);
        }

        [Fact]
        public async Task GetTradesAsync_ShouldReturnAllTrades()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _tradeService.GetTradesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetTradesAsync_WithUserId_ShouldReturnUserTrades()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _tradeService.GetTradesAsync("user123");

            // Assert
            Assert.Single(result);
            Assert.Equal("user123", result.First().UserId);
        }

        private async Task SeedTestData()
        {
            var trades = new[]
            {
                new Trade { Symbol = "AAPL", Quantity = 100, Price = 150m, Type = TradeType.Buy, UserId = "user123" },
                new Trade { Symbol = "GOOGL", Quantity = 50, Price = 2500m, Type = TradeType.Sell, UserId = "user456" }
            };

            _context.Trades.AddRange(trades);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}