using Microsoft.AspNetCore.Mvc;
using TradingService.Core.DTOs;
using TradingService.Core.Interfaces;

namespace TradingService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradesController : ControllerBase
    {
        private readonly ITradeService _tradeService;
        private readonly ILogger<TradesController> _logger;

        public TradesController(ITradeService tradeService, ILogger<TradesController> logger)
        {
            _tradeService = tradeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<TradeResponseDto>> ExecuteTrade([FromBody] CreateTradeDto tradeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tradeService.ExecuteTradeAsync(tradeDto);
                return CreatedAtAction(nameof(GetTrade), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing trade");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TradeResponseDto>>> GetTrades([FromQuery] string? userId = null)
        {
            try
            {
                var trades = await _tradeService.GetTradesAsync(userId);
                return Ok(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trades");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TradeResponseDto>> GetTrade(int id)
        {
            try
            {
                var trade = await _tradeService.GetTradeByIdAsync(id);
                return trade != null ? Ok(trade) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trade {TradeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
