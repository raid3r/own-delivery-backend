using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }
    ICourierRepository Couriers { get; }
    ICustomerRepository Customers { get; }
    ITariffRepository Tariffs { get; }
    IRatingRepository Ratings { get; }
    IPaymentRepository Payments { get; }
    INotificationRepository Notifications { get; }
    IRepository<CourierLocation> CourierLocations { get; }
    IRepository<CourierDocument> CourierDocuments { get; }
    IRepository<OrderStatusHistory> OrderStatusHistories { get; }
    
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<bool> BeginTransactionAsync(CancellationToken ct = default);
    Task<bool> CommitTransactionAsync(CancellationToken ct = default);
    Task<bool> RollbackTransactionAsync(CancellationToken ct = default);
}
