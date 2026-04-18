using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

public record CreateOrderRequest(
    AddressDto PickupAddress,
    AddressDto DeliveryAddress,
    decimal Weight,
    DimensionsDto Dimensions,
    Guid TariffId,
    string? Description = null,
    string? SpecialInstructions = null,
    DateTime? ScheduledDeliveryTime = null,
    PaymentMethod? PaymentMethod = null);
