namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for updating editable order fields.
/// </summary>
/// <param name="Description">Updated order description.</param>
/// <param name="SpecialInstructions">Updated special instructions for courier.</param>
/// <param name="ScheduledDeliveryTime">Updated scheduled UTC delivery time.</param>
public record UpdateOrderRequest(
    string? Description = null,
    string? SpecialInstructions = null,
    DateTime? ScheduledDeliveryTime = null);
