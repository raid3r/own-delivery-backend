namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for courier registration.
/// </summary>
/// <remarks>
/// Password must be at least 8 characters and contain uppercase, lowercase and digit.
/// Phone number must follow E.164 format, e.g. <c>+380501234567</c>.
/// </remarks>
/// <param name="FirstName">Courier first name.</param>
/// <param name="LastName">Courier last name.</param>
/// <param name="Email">Courier email address.</param>
/// <param name="Password">Courier password.</param>
/// <param name="PhoneNumber">Courier phone number.</param>
/// <param name="LicenseNumber">Optional driving or professional license number.</param>
public record RegisterCourierRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber,
    string? LicenseNumber = null);
