using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for creating a new delivery order.
/// </summary>
/// <remarks>
/// <c>TariffId</c> must reference an active tariff. Weight and dimensions must not exceed
/// the tariff's <c>MaxWeight</c> and <c>MaxDimensions</c> limits, otherwise the request will be rejected.
/// </remarks>
/// <param name="PickupAddress">Pickup address details.</param>
/// <param name="DeliveryAddress">Delivery address details.</param>
/// <param name="Weight">Package weight in kilograms.</param>
/// <param name="Dimensions">Package dimensions.</param>
/// <param name="TariffId">Selected tariff identifier.</param>
/// <param name="Description">Optional package description.</param>
/// <param name="SpecialInstructions">Optional instructions for courier handling.</param>
/// <param name="ScheduledDeliveryTime">Optional scheduled UTC delivery time.</param>
/// <param name="PaymentMethod">Optional preferred payment method.</param>
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
