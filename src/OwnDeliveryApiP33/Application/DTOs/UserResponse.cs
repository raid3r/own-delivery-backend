namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Generic user profile information.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">User email address.</param>
/// <param name="FullName">User full name.</param>
/// <param name="PhoneNumber">User phone number.</param>
/// <param name="Role">User role name.</param>
/// <param name="Status">Current user status name.</param>
/// <param name="IsEmailVerified">Indicates whether email is verified.</param>
/// <param name="AvatarUrl">Optional avatar image URL.</param>
public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string PhoneNumber,
    string Role,
    string Status,
    bool IsEmailVerified,
    string? AvatarUrl = null);
