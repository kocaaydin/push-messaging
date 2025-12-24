using Push.Messaging.Data.Interfaces;
using Push.Messaging.Shared.Dtos;
using Push.Messaging.Shared.Interfaces;

namespace Push.Messaging.Api.Workers;

public class UserCacheRefreshWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IUserCache _cache;

    public UserCacheRefreshWorker(
        IServiceScopeFactory scopeFactory,
        IUserCache cache)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider
                .GetRequiredService<IUserRepository>();

            var users = await userRepository.GetAllAsync();

            foreach (var user in users)
            {
                await _cache.SetAsync(new UserCacheDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    IsActive = user.IsActive
                });
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
