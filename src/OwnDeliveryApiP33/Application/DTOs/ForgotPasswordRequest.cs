namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for starting password reset flow.
/// </summary>
/// <param name="Email">Email address of the account.</param>
public record ForgotPasswordRequest(string Email);
