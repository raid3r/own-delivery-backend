# ?? Step 03: Repository Pattern & Data Access Layer

## ?? Мета
Реалізувати Repository Pattern для абстракції доступу до даних та забезпечити чисту архітектуру.

## ?? Описання
Цей крок створює generic repository для базових CRUD операцій та специфічні репозиторії для кожної entity з кастомними запитами.

---

## ? Вимоги до виконання

### 1. **Generic Repository** (папка: `Infrastructure/Repositories/`)

#### a. **IRepository<T>.cs** (Interface)
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(T entity);
    Task SaveChangesAsync();
    IQueryable<T> GetQueryable();
}
```

#### b. **Repository<T>.cs** (Implementation)
```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    // Реалізація всіх методів з async/await
    // Використання IQueryable для performance
}
```

### 2. **Specific Repositories** (папка: `Infrastructure/Repositories/`)

Для кожної main entity створіть специфічний репозиторій з кастомною логікою:

#### a. **IUserRepository.cs**
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludeUserId = null);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status);
}
```

#### b. **IOrderRepository.cs**
```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetCustomerOrdersAsync(Guid customerId, int skip = 0, int take = 20);
    Task<IEnumerable<Order>> GetCourierOrdersAsync(Guid courierId, int skip = 0, int take = 20);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int skip = 0, int take = 20);
    Task<IEnumerable<Order>> GetUnassignedOrdersAsync();
    Task<IEnumerable<Order>> GetOverdueOrdersAsync();
    Task<int> GetCustomerOrderCountAsync(Guid customerId);
    Task<decimal> GetAverageCostAsync();
}
```

#### c. **ICourierRepository.cs**
```csharp
public interface ICourierRepository : IRepository<Courier>
{
    Task<Courier?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Courier>> GetAvailableCouriersAsync(int skip = 0, int take = 20);
    Task<IEnumerable<Courier>> GetVerifiedCouriersAsync();
    Task<IEnumerable<Courier>> GetCouriersByStatusAsync(CourierStatus status);
    Task<Courier?> GetNearestCourierAsync(decimal latitude, decimal longitude, double radiusKm = 10);
    Task<IEnumerable<Courier>> GetCouriersByRatingAsync(decimal minRating, int skip = 0, int take = 20);
    Task<int> GetTotalDeliveriesAsync(Guid courierId);
}
```

#### d. **ITariffRepository.cs**
```csharp
public interface ITariffRepository : IRepository<Tariff>
{
    Task<IEnumerable<Tariff>> GetActiveTariffsAsync();
    Task<Tariff?> GetByNameAsync(string name);
    Task<Tariff?> GetDefaultTariffAsync();
}
```

#### e. **ICustomerRepository.cs**
```csharp
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Customer>> GetTopCustomersAsync(int count = 10);
    Task<decimal> GetTotalSpentAsync(Guid customerId);
}
```

#### f. **IRatingRepository.cs**
```csharp
public interface IRatingRepository : IRepository<Rating>
{
    Task<IEnumerable<Rating>> GetCourierRatingsAsync(Guid courierId);
    Task<IEnumerable<Rating>> GetOrderRatingsAsync(Guid orderId);
    Task<decimal> GetAverageRatingAsync(Guid courierId);
    Task<int> GetRatingCountAsync(Guid courierId);
}
```

#### g. **IPaymentRepository.cs**
```csharp
public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<Payment?> GetByTransactionIdAsync(string transactionId);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
    Task<IEnumerable<Payment>> GetCompletedPaymentsAsync(DateTime from, DateTime to);
}
```

#### h. **INotificationRepository.cs**
```csharp
public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
}
```

### 3. **Unit of Work Pattern** (папка: `Infrastructure/Repositories/`)

#### a. **IUnitOfWork.cs** (Interface)
```csharp
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
    
    Task SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}
```

#### b. **UnitOfWork.cs** (Implementation)
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    // Lazy initialization для repositories
    private IUserRepository? _userRepository;
    private IOrderRepository? _orderRepository;
    // ... інші репозиторії
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public IUserRepository Users => 
        _userRepository ??= new UserRepository(_context);
    
    public IOrderRepository Orders => 
        _orderRepository ??= new OrderRepository(_context);
    
    // ... інші properties
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return true;
    }
    
    public async Task<bool> CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction?.CommitAsync()!;
            return true;
        }
        catch
        {
            await RollbackTransactionAsync();
            return false;
        }
    }
    
    public async Task<bool> RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync()!;
            return true;
        }
        finally
        {
            await _transaction?.DisposeAsync()!;
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}
```

### 4. **Dependency Injection** (Program.cs)

```csharp
// Додати в Program.cs:
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

---

## ?? Unit Tests

Створіть tests у `OwnDeliveryApiP33.Tests.Unit/Infrastructure/Repositories/`:

### 1. **Generic Repository Tests**
- ? AddAsync works correctly
- ? GetByIdAsync retrieves correct entity
- ? GetAllAsync returns all entities
- ? FindAsync with predicate works
- ? UpdateAsync updates entity correctly
- ? DeleteAsync removes entity
- ? ExistsAsync checks existence
- ? CountAsync returns correct count

### 2. **Specific Repository Tests**
- ? GetByEmailAsync finds user by email
- ? GetCustomerOrdersAsync returns user's orders with pagination
- ? GetAvailableCouriersAsync returns only available couriers
- ? GetUnassignedOrdersAsync returns orders without courier
- ? GetNearestCourierAsync returns closest courier

### 3. **Unit of Work Tests**
- ? SaveChangesAsync commits changes
- ? BeginTransactionAsync starts transaction
- ? CommitTransactionAsync commits successfully
- ? RollbackTransactionAsync rolls back changes
- ? All repositories are available

### 4. **Performance Tests** (Optional)
- ? Queries are efficient (no N+1)
- ? Pagination works correctly
- ? Large result sets are handled

---

## ?? Очікувані результати

1. ? Generic Repository реалізований
2. ? Всі специфічні репозиторії створені
3. ? Unit of Work pattern реалізований
4. ? DI налаштований в Program.cs
5. ? Unit test покриття >85%
6. ? Немає компіляційних помилок
7. ? Всі операції async/await

---

## ?? Критерії приймання

- [ ] Generic Repository повністю реалізований
- [ ] Всі специфічні репозиторії мають необхідні методи
- [ ] Unit of Work реалізований з транзакціями
- [ ] DI сконфігурований
- [ ] Unit test покриття >80%
- [ ] Немає SQL errors
- [ ] Async/await використовується правильно

---

## ?? Файли для створення

```
Infrastructure/
??? Repositories/
    ??? IRepository.cs
    ??? Repository.cs
    ??? IUserRepository.cs
    ??? UserRepository.cs
    ??? IOrderRepository.cs
    ??? OrderRepository.cs
    ??? ICourierRepository.cs
    ??? CourierRepository.cs
    ??? ICustomerRepository.cs
    ??? CustomerRepository.cs
    ??? ITariffRepository.cs
    ??? TariffRepository.cs
    ??? IRatingRepository.cs
    ??? RatingRepository.cs
    ??? IPaymentRepository.cs
    ??? PaymentRepository.cs
    ??? INotificationRepository.cs
    ??? NotificationRepository.cs
    ??? IUnitOfWork.cs
    ??? UnitOfWork.cs

OwnDeliveryApiP33.Tests.Unit/
??? Infrastructure/
    ??? Repositories/
        ??? RepositoryTests.cs
        ??? OrderRepositoryTests.cs
        ??? UserRepositoryTests.cs
        ??? CourierRepositoryTests.cs
        ??? UnitOfWorkTests.cs
```

---

## ?? Примітки для розробника

1. **Lazy Initialization в UnitOfWork:**
   ```csharp
   private IUserRepository? _userRepository;
   public IUserRepository Users => 
       _userRepository ??= new UserRepository(_context);
   ```

2. **Include/ThenInclude для eager loading:**
   ```csharp
   return await _dbSet
       .Include(o => o.Customer)
       .Include(o => o.Courier)
       .Include(o => o.Ratings)
       .FirstOrDefaultAsync(o => o.Id == id);
   ```

3. **AsNoTracking для read-only:**
   ```csharp
   return await _dbSet.AsNoTracking()
       .Where(predicate)
       .ToListAsync();
   ```

4. **Specification Pattern** (Advanced):
   - Розглядайте для складних запитів

5. **Error Handling:**
   - Логуйте SQL exceptions
   - Повертайте meaningful error messages

---

**Крок 3 з 12**  
**Статус:** ?? Готово до виконання  
**Очікуваний час:** 6-8 годин
