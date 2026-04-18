namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Detailed courier profile returned by courier endpoints.
/// </summary>
/// <param name="CourierId">Courier identifier.</param>
/// <param name="Email">Courier email address.</param>
/// <param name="FirstName">Courier first name.</param>
/// <param name="LastName">Courier last name.</param>
/// <param name="PhoneNumber">Courier phone number.</param>
/// <param name="CreatedAt">UTC date and time when courier account was created.</param>
/// <param name="IsActive">Indicates whether the courier is active.</param>
public record CourierProfileResponse(
    Guid CourierId,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime CreatedAt,
    bool IsActive);
