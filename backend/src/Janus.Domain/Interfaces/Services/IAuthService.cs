using Janus.Domain.Dtos;

namespace Janus.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<string> LoginAsync(LoginDto loginDto);
    Task<Guid> SignupAsync(SignUpDto signupDto);
}