namespace Push.Messaging.Shared.Responses;

public class GenerateTokenResponse
{
    public bool IsSuccess { get; set; }
    public string AccessToken { get; set; } = string.Empty;
}
