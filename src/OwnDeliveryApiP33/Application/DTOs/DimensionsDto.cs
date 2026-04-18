namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Package dimensions in centimeters.
/// </summary>
/// <param name="Width">Package width.</param>
/// <param name="Length">Package length.</param>
/// <param name="Height">Package height.</param>
public record DimensionsDto(
    decimal Width,
    decimal Length,
    decimal Height);
