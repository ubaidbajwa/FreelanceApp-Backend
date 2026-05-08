using FreelanceApp.Application.Exceptions;
using FreelanceApp.Application.Features.Auth.DTOs;
using FreelanceApp.Application.Interfaces.Repositories;
using FreelanceApp.Application.Interfaces.Services;
using FreelanceApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FreelanceApp.Application.Features.Auth.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        logger.LogInformation("Register attempt for email: {Email}", dto.Email);

        // Step 1: Email already exists check
        if (await userRepository.EmailExistsAsync(dto.Email))
        {
            logger.LogWarning("Registration failed - email already exists: {Email}", dto.Email);
            throw new ConflictException("Email is already registered");
        }

        // Step 2: Password hash karo
        var hashedPassword = passwordHasher.HashPassword(dto.Password);

        // Step 3: User entity banao
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = hashedPassword,
            FullName = dto.FullName.Trim(),
            Role = dto.Role,
            IsCnicVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        // Step 4: Save to database
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        logger.LogInformation("User registered successfully: {UserId}", user.Id);

        // Step 5: Response banao (no PasswordHash)
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            IsCnicVerified = user.IsCnicVerified,
            CreatedAt = user.CreatedAt
        };
    }
}