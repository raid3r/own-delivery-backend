namespace OwnDeliveryApiP33.Application.DTOs;

public record UpdateOrderRequest(
    string? Description = null,
    string? SpecialInstructions = null,
    DateTime? ScheduledDeliveryTime = null);
