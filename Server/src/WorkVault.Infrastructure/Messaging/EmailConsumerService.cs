using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Messaging;

namespace WorkVault.Infrastructure.Messaging;

/// <summary>
/// Background worker that consumes email messages from RabbitMQ and sends them.
/// Runs for the lifetime of the app.
/// </summary>
public class EmailConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailConsumerService> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public EmailConsumerService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<EmailConsumerService> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = _configuration["RabbitMQ:ConnectionString"]!;
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await RabbitMqTopology.DeclareAsync(_channel, stoppingToken);

        // Only give this consumer one message at a time until it acks.
        // Prevents one worker from grabbing the whole queue at once.
        await _channel.BasicQosAsync(0, prefetchCount: 1, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            try
            {
                var message = JsonSerializer.Deserialize<EmailMessage>(json);
                if (message is not null)
                {
                    // Resolve a scoped email sender per message
                    using var scope = _serviceProvider.CreateScope();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                    await emailSender.SendAsync(message, stoppingToken);
                }

                // Acknowledge — tell RabbitMQ this message is handled, delete it
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process email message: {Json}", json);

                // Don't requeue — a failed message would otherwise loop forever.
                // (Proper fix: dead-letter queue for inspection/retry.)
                await _channel.BasicNackAsync(
                    eventArgs.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: RabbitMqTopology.MainQueue,
            autoAck: false,          // manual ack — we ack only after success
            consumer: consumer,
            cancellationToken: stoppingToken);

        // Keep the service alive until shutdown
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}