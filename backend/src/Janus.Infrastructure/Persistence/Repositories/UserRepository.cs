using Janus.Application.Common.Interfaces;
using Janus.Domain.Entities;
using Janus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Janus.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly JanusDbContext _dbContext;

    public UserRepository(JanusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }
}
