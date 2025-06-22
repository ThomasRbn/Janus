using Janus.Application;
using Janus.Application.Common.Interfaces;
using Janus.Infrastructure.Persistence.Repositories;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Janus.Infrastructure.Persistence;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));

var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                     $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                     $"Database={Environment.GetEnvironmentVariable("DB_DATABASE")};" +
                     $"Username={Environment.GetEnvironmentVariable("DB_USERNAME")};" +
                     $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
builder.Services.AddDbContext<JanusDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllers(); // Ajout des contrôleurs
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers(); // Map des contrôleurs

app.Run();
