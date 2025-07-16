using Janus.Domain.Entities;
using Janus.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Janus.Domain.Exceptions.Auth;

namespace Janus.API.Services.Auth;

public class LocalAuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<LocalAuthService> _logger;

    public LocalAuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<LocalAuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new AuthenticationException("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                throw new AuthenticationException("Invalid credentials");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("User {Email} logged in successfully", email);
            return user.Id.ToString();
        }
        catch (AuthenticationException)
        {
            _logger.LogWarning("Authentication failed for user {Email}", email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Email}", email);
            throw new AuthenticationException("An unexpected error occurred during login", ex);
        }
    }

    public async Task<Guid> SignupAsync(string email, string password, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        try
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException(email);
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new UserCreationException(errors);
            }

            _logger.LogInformation("User {Email} created successfully", email);
            return user.Id;
        }
        catch (UserAlreadyExistsException)
        {
            _logger.LogWarning("Signup attempt with existing email {Email}", email);
            throw;
        }
        catch (UserCreationException)
        {
            _logger.LogError("User creation failed for {Email}", email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during signup for user {Email}", email);
            throw new UserCreationException("An unexpected error occurred during user creation");
        }
    }
}