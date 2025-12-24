using Microsoft.EntityFrameworkCore;
using Push.Messaging.Data.Entities;

namespace Push.Messaging.Data;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}