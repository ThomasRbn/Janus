using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Janus.Domain.Entities;
using Janus.API.Dtos.Auth;
using Janus.API.Services.Auth;
using Janus.Domain.Interfaces.Services;

namespace Janus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IAuthService _authService;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IAuthService authService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _authService = authService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
        {
            return BadRequest(new { Error = "All fields are required." });
        }

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        // Optionnel : ne pas retourner l'objet user complet (éviter d'exposer des infos sensibles)
        return CreatedAtAction(nameof(SignUp), new { id = user.Id }, new { user.Id, user.Email, user.FirstName, user.LastName });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { Error = "Email and password are required." });
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Toujours un message générique pour ne pas révéler si l'email existe
            return Unauthorized(new { Error = "Invalid email or password." });
        }

        var result = await _signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Unauthorized(new { Error = "Account is locked. Please try again later." });
        }
        if (result.IsNotAllowed)
        {
            return Unauthorized(new { Error = "User is not allowed to sign in." });
        }
        if (!result.Succeeded)
        {
            return Unauthorized(new { Error = "Invalid email or password." });
        }

        // Optionnel : ne pas retourner d'infos sensibles
        return Ok(new { Message = "Login successful", UserId = user.Id });
    }
}