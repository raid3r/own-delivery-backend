namespace OwnDeliveryApiP33.Application.DTOs;

public record RateOrderRequest(
    int Score,
    string? Comment = null);
