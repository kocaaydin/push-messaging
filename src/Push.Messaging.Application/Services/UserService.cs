using Push.Messaging.Application.Interfaces;
using Push.Messaging.Data.Interfaces;
using Push.Messaging.Shared.Dtos;
using Push.Messaging.Shared.Interfaces;

namespace Push.Messaging.Application.Services;

public class UserService : IUserService
{
    private readonly IUserCache _userCache;
    private readonly IUserRepository _repository;

    public UserService(IUserCache userCache, IUserRepository repository )
    {
        _userCache = userCache;
        _repository = repository;
    }

    public async Task<bool> IsActiveAsync(string userName)
    {
        var user = await _userCache.GetAsync(userName);

        if (user == null)
        {
            var dbUser = await _repository.GetByUserNameAsync(userName);
            if (dbUser == null)
                return false;

            user = new UserCacheDto
            {
                Id = dbUser.Id,
                UserName = dbUser.UserName,
                IsActive = dbUser.IsActive
            };

            await _userCache.SetAsync(user);
        }
        
        return user is not null && user.IsActive;
    }
}