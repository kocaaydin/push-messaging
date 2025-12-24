namespace Push.Messaging.Shared.Messaging;

public class NotificationMessage
{
    public int UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
}
