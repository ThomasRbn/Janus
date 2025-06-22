using Janus.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Janus.Application.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly UserManager<User> _userManager;

    public CreateUserCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            // In a real app, you'd use a custom, more specific exception
            throw new Exception("A user with this email already exists.");
        }

        // 2. Create the user entity
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email // It's common practice to use the email as the username
        };

        // 3. Use UserManager to create the user.
        // This handles validation, password hashing, and saving to the database.
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("\n", result.Errors.Select(e => e.Description));
            // In a real app, you would have a more structured error response
            throw new Exception($"Failed to create user: {errors}");
        }

        // 4. Return the new user's Id
        return user.Id;
    }
}
