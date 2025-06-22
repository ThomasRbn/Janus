using Janus.Application.Common.Interfaces;
using Janus.Domain.Entities;
using MediatR;

namespace Janus.Application.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation (Application Level)
        var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            // In a real app, you'd use a custom, more specific exception
            throw new Exception("A user with this email already exists.");
        }

        // 2. Create Entity (Domain Level)
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password // In a real app, you would hash the password here!
        );

        // 3. Persist
        await _userRepository.AddAsync(user);

        // 4. Return Result
        return user.Id;
    }
}
