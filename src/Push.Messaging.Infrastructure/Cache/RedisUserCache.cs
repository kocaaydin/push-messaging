using System.Text.Json;
using StackExchange.Redis;
using Push.Messaging.Shared.Interfaces;
using Push.Messaging.Shared.Dtos;

namespace Push.Messaging.Infrastructure.Cache;

public class RedisUserCache : IUserCache
{
    private readonly IDatabase _redis;

    public RedisUserCache(IConnectionMultiplexer mux)
    {
        _redis = mux.GetDatabase();
    }

    public async Task<UserCacheDto?> GetAsync(string userName)
    {
        var value = await _redis.StringGetAsync($"user:{userName}");
        return value.HasValue
            ? JsonSerializer.Deserialize<UserCacheDto>(value!)
            : null;
    }

    public async Task SetAsync(UserCacheDto user)
    {
        await _redis.StringSetAsync(
            $"user:{user.UserName}",
            JsonSerializer.Serialize(user),
            TimeSpan.FromMinutes(5));
    }
}