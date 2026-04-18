namespace OwnDeliveryApiP33.Domain.Enums;

public enum OrderStatus
{
    New = 0,
    Accepted = 1,
    WaitingForCourier = 2,
    InTransit = 3,
    Delivered = 4,
    Cancelled = 5,
    Failed = 6
}
