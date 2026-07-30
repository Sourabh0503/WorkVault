using RabbitMQ.Client;

namespace WorkVault.Infrastructure.Messaging;

/// <summary>
/// Declares the email queue topology (main queue + dead-letter exchange/queue).
/// Both the publisher and consumer call this so their declarations always match —
/// RabbitMQ rejects a redeclare with mismatched arguments, so one source of truth
/// avoids drift.
/// </summary>
public static class RabbitMqTopology
{
    public const string MainQueue = "email-jobs";
    public const string DeadLetterExchange = "email-dlx";
    public const string DeadLetterQueue = "email-jobs-dead";

    public static async Task DeclareAsync(IChannel channel, CancellationToken ct = default)
    {
        // 1. Dead-letter exchange (fanout: send to every bound queue)
        await channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchange,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: ct);

        // 2. Dead-letter queue — where failed messages wait
        await channel.QueueDeclareAsync(
            queue: DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        // 3. Bind the dead queue to the dead exchange
        await channel.QueueBindAsync(
            queue: DeadLetterQueue,
            exchange: DeadLetterExchange,
            routingKey: "",
            cancellationToken: ct);

        // 4. Main queue — configured to dead-letter into the exchange above
        var args = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = DeadLetterExchange
        };

        await channel.QueueDeclareAsync(
            queue: MainQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args,
            cancellationToken: ct);
    }
}