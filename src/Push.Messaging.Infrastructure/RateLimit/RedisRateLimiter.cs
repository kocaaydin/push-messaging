using StackExchange.Redis;

namespace Push.Messaging.Infrastructure.RateLimit;

public class RedisRateLimiter
{
    private readonly IDatabase _redis;
    private readonly string _script;

    public RedisRateLimiter(IConnectionMultiplexer mux)
    {
        _redis = mux.GetDatabase();
        
        _script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "rate_limit.lua"));
    }

    public async Task<bool> AllowAsync(string key, int limit)
    {
        var result = (int)await _redis.ScriptEvaluateAsync(
            _script,
            new RedisKey[] { key },
            new RedisValue[] { limit, 1 });

        return result == 1;
    }
}