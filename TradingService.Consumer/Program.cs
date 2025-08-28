using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TradingService.Core.Messages;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var configuration = host.Services.GetRequiredService<IConfiguration>();

logger.LogInformation("Starting Trade Consumer...");

var factory = new ConnectionFactory()
{
    HostName = configuration.GetConnectionString("RabbitMQ") ?? "localhost"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare exchange and queue
channel.ExchangeDeclare("trading.exchange", ExchangeType.Topic, true);
channel.QueueDeclare("trade.executed.queue", true, false, false);
channel.QueueBind("trade.executed.queue", "trading.exchange", "trade.executed");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        Console.WriteLine($"[Trade Received] {json}");
        var tradeMessage = JsonSerializer.Deserialize<TradeExecutedMessage>(json);

        logger.LogInformation(
            "Trade Executed - ID: {TradeId}, User: {UserId}, Symbol: {Symbol}, " +
            "Quantity: {Quantity}, Price: {Price}, Type: {Type}, Total: {TotalValue:C}, " +
            "Executed At: {ExecutedAt}",
            tradeMessage?.TradeId,
            tradeMessage?.UserId,
            tradeMessage?.Symbol,
            tradeMessage?.Quantity,
            tradeMessage?.Price,
            tradeMessage?.Type,
            tradeMessage?.TotalValue,
            tradeMessage?.ExecutedAt
        );

        

        channel.BasicAck(ea.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing trade message");
        channel.BasicNack(ea.DeliveryTag, false, true);
    }
};

channel.BasicConsume("trade.executed.queue", false, consumer);

logger.LogInformation("Trade Consumer is running. Press [enter] to exit.");
Console.ReadLine();

logger.LogInformation("Shutting down Trade Consumer...");