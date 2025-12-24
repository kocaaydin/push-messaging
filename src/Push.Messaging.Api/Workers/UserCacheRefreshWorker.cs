using Push.Messaging.Data.Interfaces;
using Push.Messaging.Shared.Dtos;
using Push.Messaging.Shared.Interfaces;

namespace Push.Messaging.Api.Workers;

public class UserCacheRefreshWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IUserCache _cache;
    private readonly int _pageSize;

    public UserCacheRefreshWorker(
        IServiceScopeFactory scopeFactory,
        IUserCache cache,IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _pageSize = config.GetValue<int>("UserCache:PageSize");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var page = 0;

            while (true)
            {
                var users = await repo.GetPagedAsync(page, _pageSize);
                if (users.Count == 0)
                    break;

                foreach (var user in users)
                {
                    await _cache.SetAsync(new UserCacheDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        IsActive = user.IsActive
                    });
                }

                page++;
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
