namespace FreelanceApp.Application.Interfaces.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateAsync(Guid userId);
    Task<Guid?> ValidateAndConsumeAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
    Task RevokeAllForUserAsync(Guid userId);
}