namespace FreelanceApp.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsCnicVerified { get; }
    bool IsAuthenticated { get; }
}