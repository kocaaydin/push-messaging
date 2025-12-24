using Push.Messaging.Shared.Dtos;

namespace Push.Messaging.Shared.Interfaces;

public interface IUserCache
{
    Task<UserCacheDto?> GetAsync(string userName);
    Task SetAsync(UserCacheDto user);
}