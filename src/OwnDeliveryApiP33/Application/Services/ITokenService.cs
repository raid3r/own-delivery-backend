using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Application.Services;

public interface ITokenService : IApplicationService
{
    /// <summary>Generate JWT token for a user</summary>
    (string Token, DateTime ExpiresAt) GenerateToken(User user);

    /// <summary>Generate refresh token</summary>
    (string RefreshToken, DateTime ExpiresAt) GenerateRefreshToken();

    /// <summary>Validate JWT token</summary>
    bool ValidateToken(string token);

    /// <summary>Get principal from token (for refresh)</summary>
    System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
