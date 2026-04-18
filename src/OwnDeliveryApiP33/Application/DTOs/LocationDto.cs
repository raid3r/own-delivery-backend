namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Geo-location information captured during delivery.
/// </summary>
/// <param name="Latitude">Latitude coordinate.</param>
/// <param name="Longitude">Longitude coordinate.</param>
/// <param name="Accuracy">Optional location accuracy in meters.</param>
/// <param name="Altitude">Optional altitude in meters.</param>
/// <param name="Speed">Optional speed in meters per second.</param>
public record LocationDto(
    decimal Latitude,
    decimal Longitude,
    decimal? Accuracy = null,
    decimal? Altitude = null,
    decimal? Speed = null);
