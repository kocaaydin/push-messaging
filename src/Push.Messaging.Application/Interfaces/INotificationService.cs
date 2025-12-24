using System;
using Push.Messaging.Shared.Dtos;

namespace Push.Messaging.Application.Interfaces;

public interface INotificationService
{
    Task PublishAsync(int userId, NotificationDto dto);
}
