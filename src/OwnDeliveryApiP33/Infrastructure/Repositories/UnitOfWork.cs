using Microsoft.EntityFrameworkCore.Storage;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization for repositories
    private IUserRepository? _userRepository;
    private IOrderRepository? _orderRepository;
    private ICourierRepository? _courierRepository;
    private ICustomerRepository? _customerRepository;
    private ITariffRepository? _tariffRepository;
    private IRatingRepository? _ratingRepository;
    private IPaymentRepository? _paymentRepository;
    private INotificationRepository? _notificationRepository;
    private IRepository<CourierLocation>? _courierLocationRepository;
    private IRepository<CourierDocument>? _courierDocumentRepository;
    private IRepository<OrderStatusHistory>? _orderStatusHistoryRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_context);

    public IOrderRepository Orders =>
        _orderRepository ??= new OrderRepository(_context);

    public ICourierRepository Couriers =>
        _courierRepository ??= new CourierRepository(_context);

    public ICustomerRepository Customers =>
        _customerRepository ??= new CustomerRepository(_context);

    public ITariffRepository Tariffs =>
        _tariffRepository ??= new TariffRepository(_context);

    public IRatingRepository Ratings =>
        _ratingRepository ??= new RatingRepository(_context);

    public IPaymentRepository Payments =>
        _paymentRepository ??= new PaymentRepository(_context);

    public INotificationRepository Notifications =>
        _notificationRepository ??= new NotificationRepository(_context);

    public IRepository<CourierLocation> CourierLocations =>
        _courierLocationRepository ??= new Repository<CourierLocation>(_context);

    public IRepository<CourierDocument> CourierDocuments =>
        _courierDocumentRepository ??= new Repository<CourierDocument>(_context);

    public IRepository<OrderStatusHistory> OrderStatusHistories =>
        _orderStatusHistoryRepository ??= new Repository<OrderStatusHistory>(_context);

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
        return _transaction != null;
    }

    public async Task<bool> CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_transaction == null)
                return false;

            await SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
            return true;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task<bool> RollbackTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_transaction == null)
                return false;

            await _transaction.RollbackAsync(ct);
            return true;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}
