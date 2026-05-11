using StackExchange.Redis;
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
using FreelanceApp.Api.ExceptionHandlers;
using FreelanceApp.Application.Common.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FreelanceApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Database connection setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis connection (singleton — manages internal connection pool)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis")!;
    return ConnectionMultiplexer.Connect(connectionString);
});

// Refresh token service
builder.Services.AddScoped<IRefreshTokenService, RedisRefreshTokenService>();

// Repository registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();


// AuthService registration
builder.Services.AddScoped<IAuthService, AuthService>();

// Global exception handling (RFC 7807 ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// JWT Settings binding
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// JWT Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// JWT Authentication
var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };
    });

// HttpContext access (required for CurrentUserService)
builder.Services.AddHttpContextAccessor();

// Current user service (Clean Architecture pattern)
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Authorization with custom policies
builder.Services.AddAuthorization(options =>
{
    // Pakistan-focused KYC policy
    options.AddPolicy("CnicVerified", policy =>
        policy.RequireClaim("cnic_verified", "true"));

    // Future policies (placeholders for next phases)
    options.AddPolicy("FreelancerOnly", policy =>
        policy.RequireClaim("role", "Freelancer"));

    options.AddPolicy("ClientOnly", policy =>
        policy.RequireClaim("role", "Client"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "Admin"));
});

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

// Exception handler MUST be first in pipeline
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
