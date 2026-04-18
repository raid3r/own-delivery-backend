using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

public record CourierResponse(
    Guid Id,
    string Email,
    string FullName,
    string PhoneNumber,
    bool IsVerified,
    CourierStatus CurrentStatus,
    decimal AverageRating,
    int TotalDeliveries,
    int CompletedDeliveries,
    string? LicenseNumber = null);
