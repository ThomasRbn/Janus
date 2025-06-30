using Janus.Application;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Janus.Infrastructure.Persistence;
using Janus.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));

var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                     $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                     $"Database={Environment.GetEnvironmentVariable("DB_DATABASE")};" +
                     $"Username={Environment.GetEnvironmentVariable("DB_USERNAME")};" +
                     $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
builder.Services.AddDbContext<JanusDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
    .AddEntityFrameworkStores<JanusDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


if (builder.Environment.IsProduction())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/var/keys/"))
        .SetApplicationName("Janus");

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
