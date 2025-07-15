using Janus.Domain.Entities;
using Janus.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Janus.Infrastructure.Persistence;

namespace Janus.API.Services.Auth;

public class UserAuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JanusDbContext _context;
    private readonly ILogger<UserAuthService> _logger;

    public UserAuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JanusDbContext context,
        ILogger<UserAuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Connexion de l'utilisateur
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("User {Email} logged in successfully", email);
            return user.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Email}", email);
            throw;
        }
    }

    public async Task<Guid> SignupAsync(string email, string password)
    {
        try
        {
            // Vérifier si l'utilisateur existe déjà
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = "User", // Tu peux demander ces infos via des paramètres
                LastName = "Default"
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            _logger.LogInformation("User {Email} created successfully", email);
            return user.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Signup failed for user {Email}", email);
            throw;
        }
    }
}