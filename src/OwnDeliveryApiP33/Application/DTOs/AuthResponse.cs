namespace OwnDeliveryApiP33.Application.DTOs;

public record AuthResponse(
    Guid CourierId,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt);
