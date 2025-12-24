using Push.Messaging.Application.Interfaces;
using Push.Messaging.Shared.Interfaces;

namespace Push.Messaging.Application.Services;

public class UserService : IUserService
{
    private readonly IUserCache _userCache;

    public UserService(IUserCache userCache)
    {
        _userCache = userCache;
    }

    public async Task<bool> IsActiveAsync(string userName)
    {
        var user = await _userCache.GetAsync(userName);
        return user is not null && user.IsActive;
    }
}