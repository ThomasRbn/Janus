using Janus.Domain.Entities;
using Janus.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Janus.Domain.Exceptions.Auth;
using Janus.Domain.Dtos;

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

    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        if (loginDto == null)
            throw new ArgumentNullException(nameof(loginDto));
        
        if (string.IsNullOrWhiteSpace(loginDto.Email))
            throw new ArgumentException("Email cannot be null or empty", nameof(loginDto.Email));
        
        if (string.IsNullOrWhiteSpace(loginDto.Password))
            throw new ArgumentException("Password cannot be null or empty", nameof(loginDto.Password));

        try
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new AuthenticationException("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                throw new AuthenticationException("Invalid credentials");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
            return user.Id.ToString();
        }
        catch (AuthenticationException)
        {
            _logger.LogWarning("Authentication failed for user {Email}", loginDto.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Email}", loginDto.Email);
            throw new AuthenticationException("An unexpected error occurred during login", ex);
        }
    }

    public async Task<Guid> SignupAsync(SignUpDto signupDto)
    {
        if (signupDto == null)
            throw new ArgumentNullException(nameof(signupDto));
        
        if (string.IsNullOrWhiteSpace(signupDto.Email))
            throw new ArgumentException("Email cannot be null or empty", nameof(signupDto.Email));
        
        if (string.IsNullOrWhiteSpace(signupDto.Password))
            throw new ArgumentException("Password cannot be null or empty", nameof(signupDto.Password));
        
        if (string.IsNullOrWhiteSpace(signupDto.FirstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(signupDto.FirstName));
        
        if (string.IsNullOrWhiteSpace(signupDto.LastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(signupDto.LastName));

        try
        {
            var existingUser = await _userManager.FindByEmailAsync(signupDto.Email);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException(signupDto.Email);
            }

            var user = new User
            {
                UserName = signupDto.Email,
                Email = signupDto.Email,
                FirstName = signupDto.FirstName,
                LastName = signupDto.LastName
            };

            var result = await _userManager.CreateAsync(user, signupDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new UserCreationException(errors);
            }

            _logger.LogInformation("User {Email} created successfully", signupDto.Email);
            return user.Id;
        }
        catch (UserAlreadyExistsException)
        {
            _logger.LogWarning("Signup attempt with existing email {Email}", signupDto.Email);
            throw;
        }
        catch (UserCreationException)
        {
            _logger.LogError("User creation failed for {Email}", signupDto.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during signup for user {Email}", signupDto.Email);
            throw new UserCreationException("An unexpected error occurred during user creation");
        }
    }
}