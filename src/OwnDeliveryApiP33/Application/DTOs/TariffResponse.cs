namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Tariff details used for delivery cost calculation.
/// </summary>
/// <param name="Id">Tariff identifier.</param>
/// <param name="Name">Tariff name.</param>
/// <param name="BaseCost">Base order cost.</param>
/// <param name="PricePerKm">Price per kilometer.</param>
/// <param name="PricePerKg">Price per kilogram.</param>
/// <param name="EstimatedDeliveryTime">Estimated delivery time in minutes.</param>
/// <param name="MaxWeight">Maximum supported package weight in kilograms.</param>
/// <param name="MaxDimensions">Maximum supported package dimensions.</param>
/// <param name="IsActive">Indicates whether tariff is active.</param>
/// <param name="Description">Optional tariff description.</param>
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
