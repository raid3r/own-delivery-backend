namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Represents a physical address with optional apartment and notes.
/// </summary>
/// <param name="City">City name.</param>
/// <param name="Street">Street name.</param>
/// <param name="BuildingNumber">Building or house number.</param>
/// <param name="PostalCode">Postal or ZIP code.</param>
/// <param name="Latitude">Latitude coordinate of the address.</param>
/// <param name="Longitude">Longitude coordinate of the address.</param>
/// <param name="ApartmentNumber">Apartment, suite, or unit number.</param>
/// <param name="Description">Additional address details for the courier.</param>
public record AddressDto(
    string City,
    string Street,
    string BuildingNumber,
    string PostalCode,
    decimal Latitude,
    decimal Longitude,
    string? ApartmentNumber = null,
    string? Description = null);
