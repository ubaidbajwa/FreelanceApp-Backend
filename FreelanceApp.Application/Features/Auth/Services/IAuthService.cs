using FreelanceApp.Application.Features.Auth.DTOs;

namespace FreelanceApp.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto);
    Task LogoutAsync(LogoutRequestDto dto);
    Task VerifyEmailAsync(VerifyEmailRequestDto dto);
    Task ResendOtpAsync(ResendOtpRequestDto dto);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto dto);

    Task ResetPasswordAsync(ResetPasswordRequestDto dto);
}