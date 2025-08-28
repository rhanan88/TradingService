using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using TradingService.Core.Interfaces;
using RabbitMQ.Client;


namespace TradingService.Infrastructure
{
    public class RabbitMQPublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = configuration.GetConnectionString("RabbitMQ") ?? "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare("trading.exchange", ExchangeType.Topic, true);
        }

        public async Task PublishAsync<T>(T message, string routingKey) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: "trading.exchange",
                    routingKey: routingKey,
                    basicProperties: null,
                    body: body
                );

                _logger.LogInformation("Published message to {RoutingKey}: {Message}", routingKey, json);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to {RoutingKey}", routingKey);
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
