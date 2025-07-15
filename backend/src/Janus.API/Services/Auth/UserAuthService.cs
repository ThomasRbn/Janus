using Janus.Domain.Interfaces.Services;

public class UserAuthService : IAuthService
{
    public Task<string> LoginAsync(string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> SignupAsync(string email, string password)
    {
        throw new NotImplementedException();
    }
}