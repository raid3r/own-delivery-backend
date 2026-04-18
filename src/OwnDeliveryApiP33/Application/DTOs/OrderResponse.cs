using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

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
