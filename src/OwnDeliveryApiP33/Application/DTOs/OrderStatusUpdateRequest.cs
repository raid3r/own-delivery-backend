using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>
/// Request payload for changing order status.
/// </summary>
/// <remarks>
/// Valid transitions: <c>Pending</c> → <c>Assigned</c> → <c>PickedUp</c> → <c>InTransit</c> → <c>Delivered</c>.
/// A reason is required when transitioning to <c>Cancelled</c>.
/// </remarks>
/// <param name="NewStatus">New order status value.</param>
/// <param name="Reason">Optional reason for status change.</param>
/// <param name="Location">Optional location where status update occurred.</param>
public record OrderStatusUpdateRequest(
    OrderStatus NewStatus,
    string? Reason = null,
    LocationDto? Location = null);
