namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Credentials payload for courier login endpoint.
/// </summary>
/// <param name="Email">Courier email address.</param>
/// <param name="Password">Courier password.</param>
public record LoginCourierRequest(string Email, string Password);
