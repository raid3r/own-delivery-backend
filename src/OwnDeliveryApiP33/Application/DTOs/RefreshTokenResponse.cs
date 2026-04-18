namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Response payload returned after successful token refresh.
/// </summary>
/// <param name="AccessToken">New JWT access token.</param>
/// <param name="RefreshToken">New refresh token.</param>
/// <param name="ExpiresAt">UTC timestamp when access token expires.</param>
/// <param name="TokenType">Access token type.</param>
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");
