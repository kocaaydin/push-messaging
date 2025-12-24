using Microsoft.EntityFrameworkCore;
using Push.Messaging.Data.Entities;
using Push.Messaging.Data.Interfaces;

namespace Push.Messaging.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<User>> GetPagedAsync(int page, int pageSize)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == userName);
    }
}