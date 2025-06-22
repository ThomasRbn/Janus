using Janus.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Janus.Infrastructure.Persistence;

public class JanusDbContext : DbContext
{
    public JanusDbContext(DbContextOptions<JanusDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}
