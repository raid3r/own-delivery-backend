namespace OwnDeliveryApiP33.Application.DTOs;

public record CustomerResponse(
    Guid Id,
    string Email,
    string FullName,
    string PhoneNumber,
    decimal AverageRating,
    int TotalOrders,
    int SuccessfulOrders,
    int CancelledOrders,
    string? PreferredDeliveryAddress = null);
