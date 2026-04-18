using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Courier summary information.
/// </summary>
/// <param name="Id">Courier identifier.</param>
/// <param name="Email">Courier email address.</param>
/// <param name="FullName">Courier full name.</param>
/// <param name="PhoneNumber">Courier phone number.</param>
/// <param name="IsVerified">Indicates whether the courier account is verified.</param>
/// <param name="CurrentStatus">Current courier availability status.</param>
/// <param name="AverageRating">Average courier rating value.</param>
/// <param name="TotalDeliveries">Total number of deliveries assigned.</param>
/// <param name="CompletedDeliveries">Number of successfully completed deliveries.</param>
/// <param name="LicenseNumber">Optional courier license number.</param>
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
