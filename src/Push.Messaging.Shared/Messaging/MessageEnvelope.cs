namespace Push.Messaging.Shared.Messaging;

public class MessageEnvelope<T>
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public T Payload { get; set; } = default!;
}
