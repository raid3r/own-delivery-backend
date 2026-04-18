using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

public record OrderStatusUpdateRequest(
    OrderStatus NewStatus,
    string? Reason = null,
    LocationDto? Location = null);
