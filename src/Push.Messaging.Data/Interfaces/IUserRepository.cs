using Push.Messaging.Data.Entities;

namespace Push.Messaging.Data.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUserNameAsync(string userName);
    Task<List<User>> GetPagedAsync(int page, int pageSize);
}