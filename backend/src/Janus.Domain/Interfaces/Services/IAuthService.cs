namespace Janus.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<string> LoginAsync(string email, string password);
    Task<Guid> SignupAsync(string email, string password, string firstName, string lastName);
}