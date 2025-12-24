using System;

namespace Push.Messaging.Application.Interfaces;

public interface IUserService
{
    public Task<bool> IsActiveAsync(string userName);
}
    