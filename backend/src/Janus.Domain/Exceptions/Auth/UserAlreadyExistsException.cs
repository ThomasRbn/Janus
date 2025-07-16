namespace Janus.Domain.Exceptions.Auth;

public class UserAlreadyExistsException : Exception
{
    public string Email { get; }

    public UserAlreadyExistsException(string email) 
        : base($"User with email {email} already exists.")
    {
        Email = email;
    }

    public UserAlreadyExistsException(string email, Exception innerException) 
        : base($"User with email {email} already exists.", innerException)
    {
        Email = email;
    }
}
