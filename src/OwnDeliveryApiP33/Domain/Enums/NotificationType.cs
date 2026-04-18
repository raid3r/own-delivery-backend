namespace OwnDeliveryApiP33.Domain.Enums;

public enum NotificationType
{
    OrderCreated = 0,
    OrderAccepted = 1,
    CourierAssigned = 2,
    DeliveryStarted = 3,
    DeliveryCompleted = 4,
    OrderCancelled = 5,
    PaymentConfirmed = 6,
    DocumentRejected = 7,
    SystemAlert = 8
}
