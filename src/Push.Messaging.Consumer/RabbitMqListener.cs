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

        await _channel.BasicQosAsync(0, 10, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var envelope =
                    JsonSerializer.Deserialize<MessageEnvelope<NotificationMessage>>(json)!;

                await _handler.HandleAsync(envelope.Payload);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch
            {
                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    false,
                    requeue: false,
                    stoppingToken);
            }
        };

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