namespace OwnDeliveryApiP33.Application.DTOs;

public record AddressDto(
    string City,
    string Street,
    string BuildingNumber,
    string PostalCode,
    decimal Latitude,
    decimal Longitude,
    string? ApartmentNumber = null,
    string? Description = null);
