using Push.Messaging.Application.Interfaces;
using Push.Messaging.Shared.Dtos;
using Push.Messaging.Shared.Interfaces;
using Push.Messaging.Shared.Messaging;

namespace Push.Messaging.Application.Services;

public class NotificationService: INotificationService 
{
    private readonly IMessagePublisher _publisher;

    public NotificationService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PublishAsync(int userId, NotificationDto dto)
    {
        var envelope = new MessageEnvelope<NotificationMessage>
        {
            Payload = new NotificationMessage
            {
                UserId = userId,
                Title = dto.Title,
                Body = dto.Body
            }
        };

        await _publisher.PublishAsync(envelope);
    }
}