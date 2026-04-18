namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Authentication response returned after successful register or login.
/// </summary>
/// <remarks>
/// Include the <c>Token</c> value in the <c>Authorization: Bearer {token}</c> header for all
/// subsequent requests to protected endpoints. Token expiry is indicated by <c>ExpiresAt</c>.
/// </remarks>
/// <param name="CourierId">Identifier of the authenticated courier.</param>
/// <param name="Email">Courier email address.</param>
/// <param name="FirstName">Courier first name.</param>
/// <param name="LastName">Courier last name.</param>
/// <param name="Token">JWT access token.</param>
/// <param name="ExpiresAt">UTC timestamp when the token expires.</param>
public record AuthResponse(
    Guid CourierId,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt);
