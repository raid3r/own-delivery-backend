namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for rating an order.
/// </summary>
/// <remarks>
/// Score must be between 1 (lowest) and 5 (highest).
/// Comment is optional and may be displayed in the courier's public profile.
/// </remarks>
/// <param name="Score">Rating score value.</param>
/// <param name="Comment">Optional review comment.</param>
public record RateOrderRequest(
    int Score,
    string? Comment = null);
