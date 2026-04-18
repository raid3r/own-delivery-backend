namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Rating details for courier or customer feedback.
/// </summary>
/// <param name="Id">Rating identifier.</param>
/// <param name="CourierId">Courier identifier.</param>
/// <param name="CustomerId">Customer identifier.</param>
/// <param name="Score">Rating score.</param>
/// <param name="Type">Rating type or direction.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="Comment">Optional review comment.</param>
public record RatingResponse(
    Guid Id,
    Guid CourierId,
    Guid CustomerId,
    int Score,
    string Type,
    DateTime CreatedAt,
    string? Comment = null);
