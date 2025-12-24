using Push.Messaging.Shared.Messaging;

namespace Push.Messaging.Shared.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(MessageEnvelope<T> message);
}