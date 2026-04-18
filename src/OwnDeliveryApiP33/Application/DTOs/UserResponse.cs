namespace OwnDeliveryApiP33.Application.DTOs;

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string PhoneNumber,
    string Role,
    string Status,
    bool IsEmailVerified,
    string? AvatarUrl = null);
