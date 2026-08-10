using CampusServicePortal.DTOs.Auth;

namespace CampusServicePortal.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);

        Task VerifyEmailAsync(string token);

        Task ResendVerificationEmailAsync(string email);

        Task ForgotPasswordAsync(string email);

        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}