using FreelanceApp.Application.Common.Settings;
using FreelanceApp.Application.Exceptions;
using FreelanceApp.Application.Features.Auth.DTOs;
using FreelanceApp.Application.Interfaces.Repositories;
using FreelanceApp.Application.Interfaces.Services;
using FreelanceApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreelanceApp.Application.Features.Auth.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        logger.LogInformation("Register attempt for email: {Email}", dto.Email);

        var normalizedEmail = dto.Email.ToLower().Trim();

        if (await userRepository.EmailExistsAsync(normalizedEmail))
        {
            logger.LogWarning("Registration failed - email exists: {Email}", normalizedEmail);
            throw new ConflictException("Email is already registered");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(dto.Password),
            FullName = dto.FullName.Trim(),
            Role = dto.Role,
            IsCnicVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        logger.LogInformation("Login attempt for email: {Email}", dto.Email);

        var normalizedEmail = dto.Email.ToLower().Trim();
        var user = await userRepository.GetByEmailAsync(normalizedEmail);

        // Generic error — prevent user enumeration
        if (user == null || !passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            logger.LogWarning("Login failed for email: {Email}", normalizedEmail);
            throw new UnauthorizedException("Invalid email or password");
        }

        // Adaptive re-hashing (silent security upgrade)
        if (passwordHasher.NeedsRehash(user.PasswordHash))
        {
            logger.LogInformation("Re-hashing outdated password for user: {UserId}", user.Id);
            user.PasswordHash = passwordHasher.HashPassword(dto.Password);
            await userRepository.SaveChangesAsync();
        }

        logger.LogInformation("Login successful: {UserId}", user.Id);

        return BuildAuthResponse(user);
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var token = jwtTokenService.GenerateAccessToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

        return new AuthResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            User = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                IsCnicVerified = user.IsCnicVerified,
                CreatedAt = user.CreatedAt
            }
        };
    }
}