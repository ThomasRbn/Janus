using MediatR;

namespace Janus.Application.Features.Users.Commands;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Guid>;
