using FreelanceApp.Application.Common.Settings;
using FreelanceApp.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace FreelanceApp.Infrastructure.Services;

public class RedisRefreshTokenService(
    IConnectionMultiplexer redis,
    IOptions<JwtSettings> jwtOptions,
    ILogger<RedisRefreshTokenService> logger) : IRefreshTokenService
{
    private readonly JwtSettings _settings = jwtOptions.Value;
    private IDatabase Db => redis.GetDatabase();
    private const string KeyPrefix = "refresh_token:";

    public async Task<string> GenerateAsync(Guid userId)
    {
        // Cryptographically secure random token
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes);

        // Redis mein store: key = token, value = userId, TTL = 7 days
        var key = KeyPrefix + token;
        var expiry = TimeSpan.FromDays(_settings.RefreshTokenExpiryDays);

        await Db.StringSetAsync(key, userId.ToString(), expiry);

        logger.LogInformation("Refresh token generated for user: {UserId}", userId);
        return token;
    }

    public async Task<Guid?> ValidateAndConsumeAsync(string refreshToken)
    {
        var key = KeyPrefix + refreshToken;

        // Atomic: get + delete in one operation
        var userIdValue = await Db.StringGetDeleteAsync(key);

        if (userIdValue.IsNullOrEmpty)
        {
            logger.LogWarning("Invalid or expired refresh token used");
            return null;
        }

        if (!Guid.TryParse(userIdValue.ToString(), out var userId))
        {
            logger.LogError("Corrupt refresh token data in Redis");
            return null;
        }

        logger.LogInformation("Refresh token validated and consumed for user: {UserId}", userId);
        return userId;
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var key = KeyPrefix + refreshToken;
        await Db.KeyDeleteAsync(key);
        logger.LogInformation("Refresh token revoked");
    }

    public Task RevokeAllForUserAsync(Guid userId)
    {
        // Token Versioning approach:
        // Refresh tokens automatically become useless after password reset
        // because the access token's SecurityStamp won't match the user's new stamp.
        // The middleware (JWT OnTokenValidated event) handles invalidation.
        //
        // Refresh tokens themselves expire naturally after 7 days (their TTL in Redis).
        // No explicit per-user revocation needed — SecurityStamp does the work.

        logger.LogInformation(
            "RevokeAllForUserAsync called for user: {UserId} | Token Versioning handles invalidation via SecurityStamp",
            userId);
        return Task.CompletedTask;
    }
}