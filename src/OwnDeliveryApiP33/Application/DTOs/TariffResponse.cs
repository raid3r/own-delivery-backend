namespace OwnDeliveryApiP33.Application.DTOs;

public record TariffResponse(
    Guid Id,
    string Name,
    decimal BaseCost,
    decimal PricePerKm,
    decimal PricePerKg,
    int EstimatedDeliveryTime,
    decimal MaxWeight,
    DimensionsDto MaxDimensions,
    bool IsActive,
    string? Description = null);
