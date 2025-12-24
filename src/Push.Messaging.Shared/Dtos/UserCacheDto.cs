namespace Push.Messaging.Shared.Dtos;

public class UserCacheDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
