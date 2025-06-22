using Janus.Domain.Entities;

namespace Janus.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task AddAsync(User user);
}
