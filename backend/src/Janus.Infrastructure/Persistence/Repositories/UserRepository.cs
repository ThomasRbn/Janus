using Janus.Application.Common.Interfaces;
using Janus.Domain.Entities;

namespace Janus.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private static readonly List<User> _users = new();

    public Task<User?> GetUserByEmailAsync(string email)
    {
        var user = _users.SingleOrDefault(u => u.Email == email);
        return Task.FromResult(user);
    }

    public Task AddAsync(User user)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }
}
