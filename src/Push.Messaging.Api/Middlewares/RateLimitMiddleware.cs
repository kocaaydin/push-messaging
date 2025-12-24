using Push.Messaging.Infrastructure.RateLimit;

namespace Push.Messaging.Api.Middlewares;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        RedisRateLimiter limiter)
    {
        var key = $"rl:{context.Connection.RemoteIpAddress}";

        var allowed = await limiter.AllowAsync(key, 1000);
        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }

        await _next(context);
    }
}