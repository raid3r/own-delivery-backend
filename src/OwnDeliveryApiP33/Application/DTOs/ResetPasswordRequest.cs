namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for completing password reset.
/// </summary>
/// <param name="Token">Password reset token.</param>
/// <param name="NewPassword">New password value.</param>
/// <param name="ConfirmPassword">Confirmation of the new password.</param>
public record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword);
