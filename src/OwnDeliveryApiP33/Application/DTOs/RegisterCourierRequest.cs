namespace OwnDeliveryApiP33.Application.DTOs;

public record RegisterCourierRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber,
    string? LicenseNumber = null);
