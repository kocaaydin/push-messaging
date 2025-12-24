using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Push.Messaging.Shared.Messaging;
using Push.Messaging.Infrastructure.Options;

namespace Push.Messaging.Consumer;

public class RabbitMqListener : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);

    private readonly IConnectionFactory _factory;
    private readonly RabbitMqOptions _options;
    private readonly FirebasePushHandler _handler;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqListener(
        IConnectionFactory factory,
        IOptions<RabbitMqOptions> options,
        FirebasePushHandler handler)
    {
        _factory = factory;
        _options = options.Value;
        _handler = handler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await _factory.CreateConnectionAsync(stoppingToken);
        
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(0, BatchSize, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        var buffer = new List<(ulong tag, NotificationMessage msg)>();
        var lastFlush = DateTime.UtcNow;
        var lockObj = new object();

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var envelope =
                JsonSerializer.Deserialize<MessageEnvelope<NotificationMessage>>(json)!;

            lock (lockObj)
            {
                buffer.Add((ea.DeliveryTag, envelope.Payload));
            }

            if (buffer.Count >= BatchSize ||
                DateTime.UtcNow - lastFlush >= FlushInterval)
            {
                await SendProcessAsync();
            }
        };

        async Task SendProcessAsync()
        {
            List<(ulong tag, NotificationMessage msg)> batch;

            lock (lockObj)
            {
                if (buffer.Count == 0)
                    return;

                batch = [.. buffer];
                buffer.Clear();
                lastFlush = DateTime.UtcNow;
            }

            try
            {
                await _handler.HandleBatchAsync(batch.Select(x => x.msg));

                foreach (var item in batch)
                    await _channel.BasicAckAsync(item.tag, false, stoppingToken);
            }
            catch
            {
                foreach (var item in batch)
                    await _channel.BasicNackAsync(item.tag, false, requeue: false, stoppingToken);
            }
        }


        await _channel.BasicConsumeAsync(
            queue: _options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}