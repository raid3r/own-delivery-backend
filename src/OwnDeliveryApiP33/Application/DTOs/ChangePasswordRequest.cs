namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for changing the current user's password.
/// </summary>
/// <param name="CurrentPassword">Current password.</param>
/// <param name="NewPassword">New password.</param>
/// <param name="ConfirmPassword">Confirmation of the new password.</param>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);
