using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Messaging;

namespace WorkVault.Infrastructure.Messaging;

/// <summary>
/// Publishes email messages to a RabbitMQ queue using the default exchange
/// (routing key == queue name delivers straight to that queue).
/// </summary>
public class RabbitMqEmailPublisher : IEmailPublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqEmailPublisher(IConfiguration configuration)
    {
        var connectionString = configuration["RabbitMQ:ConnectionString"]!;

        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };

        // Note: constructor does sync-over-async to establish the connection once.
        // We'll discuss whether this should be lazy/pooled shortly.
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        RabbitMqTopology.DeclareAsync(_channel).GetAwaiter().GetResult();
    }

    public async Task PublishAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // Mark the message persistent so it survives a broker restart while
        // sitting in the (durable) queue.
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await _channel.BasicPublishAsync(
            exchange: "",              // default exchange
            routingKey: RabbitMqTopology.MainQueue,    // routes to the queue of this name
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}