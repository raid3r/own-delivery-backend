namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload containing refresh token value.
/// </summary>
/// <param name="RefreshToken">Refresh token string.</param>
public record RefreshTokenRequest(string RefreshToken);
