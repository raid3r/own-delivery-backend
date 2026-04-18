namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for customer registration.
/// </summary>
/// <param name="FirstName">Customer first name.</param>
/// <param name="LastName">Customer last name.</param>
/// <param name="Email">Customer email address.</param>
/// <param name="Password">Customer password.</param>
/// <param name="PhoneNumber">Customer phone number.</param>
public record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber);
