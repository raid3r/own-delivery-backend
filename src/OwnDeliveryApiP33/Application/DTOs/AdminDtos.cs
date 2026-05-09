using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for creating a customer from the administration API.
/// </summary>
public record AdminCreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber,
    string? PreferredDeliveryAddress = null);

/// <summary>
/// Request payload for updating customer data from the administration API.
/// </summary>
public record AdminUpdateCustomerRequest(
    string? FullName = null,
    string? PhoneNumber = null,
    string? PreferredDeliveryAddress = null,
    UserStatus? Status = null);

/// <summary>
/// Request payload for creating an order from the administration API.
/// </summary>
public record AdminCreateOrderRequest(
    Guid CustomerId,
    AddressDto PickupAddress,
    AddressDto DeliveryAddress,
    decimal Weight,
    DimensionsDto Dimensions,
    Guid TariffId,
    string? Description = null,
    string? SpecialInstructions = null,
    DateTime? ScheduledDeliveryTime = null,
    PaymentMethod? PaymentMethod = null,
    OrderStatus Status = OrderStatus.Pending);

/// <summary>
/// Request payload for cancelling an order from the administration API.
/// </summary>
public record AdminCancelOrderRequest(string Reason);
