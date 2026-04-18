namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Customer summary information.
/// </summary>
/// <param name="Id">Customer identifier.</param>
/// <param name="Email">Customer email address.</param>
/// <param name="FullName">Customer full name.</param>
/// <param name="PhoneNumber">Customer phone number.</param>
/// <param name="AverageRating">Average rating received by the customer.</param>
/// <param name="TotalOrders">Total number of created orders.</param>
/// <param name="SuccessfulOrders">Number of successfully completed orders.</param>
/// <param name="CancelledOrders">Number of cancelled orders.</param>
/// <param name="PreferredDeliveryAddress">Optional preferred delivery address.</param>
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
