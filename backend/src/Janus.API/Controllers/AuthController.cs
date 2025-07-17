using Microsoft.AspNetCore.Mvc;
using Janus.Domain.Interfaces.Services;
using Janus.Domain.Exceptions.Auth;
using Janus.Domain.Dtos;

namespace Janus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
        {
            return BadRequest(new { Error = "All fields are required." });
        }

        try
        {
            var userId = await _authService.SignupAsync(dto);
            return CreatedAtAction(nameof(SignUp), new { id = userId }, new { Id = userId, Email = dto.Email, FirstName = dto.FirstName, LastName = dto.LastName });
        }
        catch (UserAlreadyExistsException ex)
        {
            return Conflict(new { Error = "User already exists", Email = ex.Email });
        }
        catch (UserCreationException ex)
        {
            return BadRequest(new { Error = "User creation failed", Errors = ex.Errors });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during signup for {Email}", dto.Email);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { Error = "Email and password are required." });
        }

        try
        {
            var userId = await _authService.LoginAsync(dto);
            return Ok(new { Message = "Login successful", UserId = userId });
        }
        catch (AuthenticationException ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
        catch (UserCreationException ex)
        {
            _logger.LogError("User creation failed during LDAP login for {Email}", dto.Email);
            return StatusCode(500, new { Error = "Failed to create user account", Errors = ex.Errors });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", dto.Email);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }
}