namespace OwnDeliveryApiP33.Application.DTOs;

public record RatingResponse(
    Guid Id,
    Guid CourierId,
    Guid CustomerId,
    int Score,
    string Type,
    DateTime CreatedAt,
    string? Comment = null);
