using Push.Messaging.Data.Entities;

namespace Push.Messaging.Data.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
}