using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Services;

public interface IAuthService : IApplicationService
{
    /// <summary>Register a new courier</summary>
    Task<AuthResponse> RegisterAsync(RegisterCourierRequest request, CancellationToken ct = default);

    /// <summary>Login as a courier and receive a JWT token</summary>
    Task<AuthResponse> LoginAsync(LoginCourierRequest request, CancellationToken ct = default);

    /// <summary>Login with email and password (generic login)</summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>Refresh access token using refresh token</summary>
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Logout user</summary>
    Task<bool> LogoutAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Verify email address with token</summary>
    Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default);

    /// <summary>Request password reset token</summary>
    Task<bool> RequestPasswordResetAsync(string email, CancellationToken ct = default);

    /// <summary>Reset password with token</summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);

    /// <summary>Change user password</summary>
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}
