using Microsoft.AspNetCore.Identity;

namespace Janus.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
