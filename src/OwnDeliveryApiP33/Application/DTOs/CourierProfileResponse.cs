namespace OwnDeliveryApiP33.Application.DTOs;

public record CourierProfileResponse(
    Guid CourierId,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime CreatedAt,
    bool IsActive);
