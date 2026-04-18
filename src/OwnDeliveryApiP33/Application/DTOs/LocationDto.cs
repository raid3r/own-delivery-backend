namespace OwnDeliveryApiP33.Application.DTOs;

public record LocationDto(
    decimal Latitude,
    decimal Longitude,
    decimal? Accuracy = null,
    decimal? Altitude = null,
    decimal? Speed = null);
