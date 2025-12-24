using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Push.Messaging.Infrastructure.Options;
using Push.Messaging.Shared.Messaging;

namespace Push.Messaging.Consumer;

public class FirebasePushHandler
{
    private readonly HttpClient _httpClient;
    private readonly FirebaseOptions _options;

    public FirebasePushHandler(
        HttpClient httpClient,
        IOptions<FirebaseOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }
    public async Task HandleBatchAsync(IEnumerable<NotificationMessage> messages)
    {
        var payload = new
        {
            messages = messages.Select(x => new
            {
                to = $"/topics/user-{x.UserId}",
                notification = new
                {
                    title = x.Title,
                    body = x.Body
                }
            })
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_options.Url, content);
        response.EnsureSuccessStatusCode();
    }
}