namespace Janus.Domain.Exceptions.Auth;

public class UserCreationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public UserCreationException(IEnumerable<string> errors) 
        : base($"User creation failed: {string.Join(", ", errors)}")
    {
        Errors = errors;
    }

    public UserCreationException(string message) : base(message)
    {
        Errors = new[] { message };
    }

    public UserCreationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new[] { message };
    }
}