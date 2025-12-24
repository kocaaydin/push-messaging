using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Push.Messaging.Shared.Interfaces;
using Push.Messaging.Shared.Messaging;
using Push.Messaging.Infrastructure.Options;

namespace Push.Messaging.Infrastructure.RateLimit;

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly IConnectionFactory _factory;
    private readonly RabbitMqOptions _options;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(
        IConnectionFactory factory,
        IOptions<RabbitMqOptions> options)
    {
        _factory = factory;
        _options = options.Value;
    }

    private async Task EnsureConnectionAsync()
    {
        if (_connection != null && _channel != null)
            return;

        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false);
    }

    public async Task PublishAsync<T>(MessageEnvelope<T> message)
    {
        await EnsureConnectionAsync();

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        await _channel!.BasicPublishAsync(
            exchange: "",
            routingKey: _options.Queue,
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}