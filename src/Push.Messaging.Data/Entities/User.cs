namespace Push.Messaging.Data.Entities;

public class User
{
    public int Id { get; set; }          
    public string UserName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
