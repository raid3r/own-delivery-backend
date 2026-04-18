using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Delivery order details returned by order endpoints.
/// </summary>
/// <param name="Id">Order identifier.</param>
/// <param name="OrderNumber">Business order number.</param>
/// <param name="Status">Current order status.</param>
/// <param name="PickupAddress">Pickup address.</param>
/// <param name="DeliveryAddress">Delivery address.</param>
/// <param name="Weight">Package weight in kilograms.</param>
/// <param name="Dimensions">Package dimensions.</param>
/// <param name="Cost">Calculated delivery cost.</param>
/// <param name="PaymentStatus">Current payment status.</param>
/// <param name="CreatedAt">UTC date and time when order was created.</param>
/// <param name="ActualDeliveryTime">UTC date and time when order was delivered.</param>
/// <param name="Description">Optional order description.</param>
/// <param name="SpecialInstructions">Optional courier instructions.</param>
public record OrderResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    AddressDto PickupAddress,
    AddressDto DeliveryAddress,
    decimal Weight,
    DimensionsDto Dimensions,
    decimal Cost,
    PaymentStatus PaymentStatus,
    DateTime CreatedAt,
    DateTime? ActualDeliveryTime = null,
    string? Description = null,
    string? SpecialInstructions = null);
