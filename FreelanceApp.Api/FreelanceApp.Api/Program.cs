using FreelanceApp.Application.Interfaces.Repositories;
using FreelanceApp.Infrastructure.Repositories;
using Freelancer.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using FreelanceApp.Application.Features.Auth.Validators;
using FreelanceApp.Application.Interfaces.Services;
using FreelanceApp.Infrastructure.Services;
using FreelanceApp.Application.Features.Auth.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Database connection setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();

// AuthService registration
builder.Services.AddScoped<IAuthService, AuthService>();

// FluentValidation registration
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Service registrations
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

// CORS setup — Flutter aur web panel ke liye
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
