# ?? Step 04: Application Services Layer

## ?? Мета
Реалізувати Application Services для бізнес-логіки та взаємодії між User та Repository layers.

## ?? Описання
Цей крок створює Service interfaces та implementations для основних бізнес-процесів: управління користувачами, замовленнями, кур'єрами та тарифами.

---

## ? Вимоги до виконання

### 1. **Base Service Interface** (папка: `Application/Services/`)

#### a. **IApplicationService.cs**
```csharp
public interface IApplicationService
{
    // Marker interface для всіх services
}
```

### 2. **User & Authentication Services** (вже мають існувати)

#### a. **IAuthService.cs** (Оновити якщо потрібно)
```csharp
public interface IAuthService : IApplicationService
{
    Task<AuthResponse> RegisterCustomerAsync(RegisterCustomerRequest request);
    Task<AuthResponse> RegisterCourierAsync(RegisterCourierRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(Guid userId);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
```

#### b. **IUserService.cs** (Новий)
```csharp
public interface IUserService : IApplicationService
{
    Task<UserResponse> GetUserByIdAsync(Guid userId);
    Task<UserResponse> GetUserByEmailAsync(string email);
    Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<bool> UpdateAvatarAsync(Guid userId, IFormFile avatar);
    Task<bool> DeleteAccountAsync(Guid userId);
    Task<bool> BlockUserAsync(Guid userId, string reason);
    Task<bool> UnblockUserAsync(Guid userId);
    Task<IEnumerable<UserResponse>> GetUsersByRoleAsync(UserRole role, int skip = 0, int take = 20);
}
```

### 3. **Customer Service** (Новий)

#### a. **ICustomerService.cs**
```csharp
public interface ICustomerService : IApplicationService
{
    Task<CustomerResponse> GetProfileAsync(Guid customerId);
    Task<CustomerResponse> UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequest request);
    Task<bool> AddSavedAddressAsync(Guid customerId, AddressDto address);
    Task<bool> RemoveSavedAddressAsync(Guid customerId, Guid addressId);
    Task<IEnumerable<AddressDto>> GetSavedAddressesAsync(Guid customerId);
    Task<bool> SetPreferredAddressAsync(Guid customerId, Guid addressId);
    Task<CustomerStatsResponse> GetStatsAsync(Guid customerId);
}
```

#### b. **Implementation**
```csharp
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;
    
    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    // Реалізація всіх методів
}
```

### 4. **Order Service** (Новий)

#### a. **IOrderService.cs**
```csharp
public interface IOrderService : IApplicationService
{
    // Customer Operations
    Task<OrderResponse> CreateOrderAsync(Guid customerId, CreateOrderRequest request);
    Task<OrderResponse> GetOrderAsync(Guid orderId);
    Task<OrderResponse> GetOrderByNumberAsync(string orderNumber);
    Task<IEnumerable<OrderResponse>> GetCustomerOrdersAsync(Guid customerId, OrderFilterRequest filter);
    Task<bool> CancelOrderAsync(Guid orderId, string reason);
    Task<bool> RateOrderAsync(Guid orderId, Guid customerId, RateOrderRequest request);
    
    // Courier Operations
    Task<IEnumerable<OrderResponse>> GetAvailableOrdersAsync(OrderFilterRequest filter);
    Task<bool> AcceptOrderAsync(Guid orderId, Guid courierId);
    Task<bool> DeclineOrderAsync(Guid orderId, Guid courierId);
    Task<IEnumerable<OrderResponse>> GetCourierOrdersAsync(Guid courierId, CourierOrderFilterRequest filter);
    
    // Order Status Management
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatusUpdateRequest request);
    Task<IEnumerable<OrderStatusHistoryResponse>> GetOrderHistoryAsync(Guid orderId);
    
    // Admin Operations
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(AdminOrderFilterRequest filter);
    Task<bool> AssignCourierAsync(Guid orderId, Guid courierId);
    Task<OrderAnalyticsResponse> GetAnalyticsAsync(AnalyticsFilterRequest filter);
}
```

#### b. **Implementation Components**
```csharp
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IOrderPricingService _pricingService;
    private readonly IOrderMatchingService _matchingService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;
    
    // Реалізація всіх методів з бізнес-логікою
}
```

### 5. **Courier Service** (Новий)

#### a. **ICourierService.cs**
```csharp
public interface ICourierService : IApplicationService
{
    // Profile Management
    Task<CourierProfileResponse> GetProfileAsync(Guid courierId);
    Task<CourierProfileResponse> UpdateProfileAsync(Guid courierId, UpdateCourierProfileRequest request);
    
    // Document Management
    Task<bool> UploadDocumentAsync(Guid courierId, UploadDocumentRequest request);
    Task<bool> RemoveDocumentAsync(Guid courierId, Guid documentId);
    Task<IEnumerable<CourierDocumentResponse>> GetDocumentsAsync(Guid courierId);
    
    // Location Management
    Task<bool> UpdateLocationAsync(Guid courierId, LocationUpdateRequest request);
    Task<LocationResponse> GetCurrentLocationAsync(Guid courierId);
    
    // Status Management
    Task<bool> UpdateStatusAsync(Guid courierId, CourierStatusUpdateRequest request);
    Task<CourierStatus> GetStatusAsync(Guid courierId);
    
    // Statistics
    Task<CourierStatsResponse> GetStatsAsync(Guid courierId);
    Task<IEnumerable<CourierRatingResponse>> GetRatingsAsync(Guid courierId);
    Task<CourierEarningsResponse> GetEarningsAsync(Guid courierId, DateTime from, DateTime to);
}
```

#### b. **Implementation**
```csharp
public class CourierService : ICourierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;
    private readonly ILocationService _locationService;
    private readonly ILogger<CourierService> _logger;
    
    // Реалізація всіх методів
}
```

### 6. **Tariff Service** (Новий)

#### a. **ITariffService.cs**
```csharp
public interface ITariffService : IApplicationService
{
    Task<TariffResponse> GetTariffByIdAsync(Guid tariffId);
    Task<IEnumerable<TariffResponse>> GetActiveTariffsAsync();
    Task<TariffResponse> CreateTariffAsync(CreateTariffRequest request);
    Task<TariffResponse> UpdateTariffAsync(Guid tariffId, UpdateTariffRequest request);
    Task<bool> DeleteTariffAsync(Guid tariffId);
    Task<decimal> CalculateCostAsync(CalculateCostRequest request);
}
```

### 7. **Payment Service** (Новий)

#### a. **IPaymentService.cs**
```csharp
public interface IPaymentService : IApplicationService
{
    Task<PaymentResponse> GetPaymentAsync(Guid paymentId);
    Task<PaymentResponse> GetOrderPaymentAsync(Guid orderId);
    Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
    Task<bool> ProcessPaymentAsync(Guid paymentId, ProcessPaymentRequest request);
    Task<bool> RefundPaymentAsync(Guid paymentId, RefundPaymentRequest request);
    Task<bool> HandlePaymentWebhookAsync(string providerName, string payload);
    Task<IEnumerable<PaymentResponse>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 20);
}
```

### 8. **Notification Service** (Новий)

#### a. **INotificationService.cs**
```csharp
public interface INotificationService : IApplicationService
{
    Task<bool> SendNotificationAsync(NotificationRequest request);
    Task<bool> SendToUserAsync(Guid userId, NotificationType type, string title, string message, Dictionary<string, string>? metadata = null);
    Task<bool> SendToMultipleAsync(IEnumerable<Guid> userIds, NotificationType type, string title, string message);
    Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
}
```

### 9. **Supporting Services**

#### a. **IOrderPricingService.cs**
```csharp
public interface IOrderPricingService
{
    Task<decimal> CalculateOrderCostAsync(Order order, Tariff tariff);
    Task<decimal> ApplyDiscountsAsync(Guid customerId, decimal baseCost);
    Task<PricingBreakdownResponse> GetPricingBreakdownAsync(PricingCalculationRequest request);
}
```

#### b. **IOrderMatchingService.cs**
```csharp
public interface IOrderMatchingService
{
    Task<Courier?> FindBestCourierAsync(Order order);
    Task<IEnumerable<Courier>> FindAvailableCouriersAsync(Order order, int maxResults = 5);
    Task<bool> AssignCourierAutomaticallyAsync(Order order);
}
```

#### c. **ILocationService.cs** (для геолокації)
```csharp
public interface ILocationService
{
    Task<decimal> CalculateDistanceAsync(Location from, Location to);
    Task<TimeSpan> EstimateDeliveryTimeAsync(Location pickup, Location delivery);
    Task<bool> IsLocationValidAsync(Location location);
}
```

#### d. **IFileStorageService.cs** (для завантаження файлів)
```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder);
    Task<bool> DeleteAsync(string fileUrl);
    Task<Stream> DownloadAsync(string fileUrl);
}
```

### 10. **Dependency Injection** (Program.cs)

```csharp
// Додати в Program.cs:
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<ITariffService, TariffService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOrderPricingService, OrderPricingService>();
builder.Services.AddScoped<IOrderMatchingService, OrderMatchingService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
```

---

## ?? Unit Tests

Створіть tests у `OwnDeliveryApiP33.Tests.Unit/Application/Services/`:

### 1. **CustomerService Tests**
- ? GetProfileAsync returns correct profile
- ? UpdateProfileAsync updates profile correctly
- ? AddSavedAddressAsync adds address
- ? GetStatsAsync calculates correct stats

### 2. **OrderService Tests**
- ? CreateOrderAsync creates valid order
- ? GetCustomerOrdersAsync returns user's orders
- ? AcceptOrderAsync assigns courier
- ? UpdateOrderStatusAsync updates status correctly
- ? CancelOrderAsync with valid reason works
- ? CancelOrderAsync fails with invalid status

### 3. **CourierService Tests**
- ? GetProfileAsync returns courier profile
- ? UpdateLocationAsync saves location
- ? UpdateStatusAsync changes status
- ? GetStatsAsync calculates correct stats
- ? UploadDocumentAsync saves document

### 4. **TariffService Tests**
- ? CreateTariffAsync creates tariff
- ? CalculateCostAsync calculates correctly
- ? GetActiveTariffsAsync returns only active

### 5. **PaymentService Tests**
- ? CreatePaymentAsync creates payment
- ? ProcessPaymentAsync updates status
- ? RefundPaymentAsync refunds successfully

---

## ?? Очікувані результати

1. ? Всі Service interfaces створені
2. ? Всі implementations реалізовані з бізнес-логікою
3. ? DI налаштований в Program.cs
4. ? Unit test покриття >80%
5. ? Немає компіляційних помилок
6. ? Async/await використовується правильно
7. ? Error handling реалізований

---

## ?? Критерії приймання

- [ ] Всі Service interfaces створені
- [ ] Всі implementations повністю реалізовані
- [ ] DI правильно налаштований
- [ ] Unit test покриття >80%
- [ ] Бізнес-логіка коректна
- [ ] Error handling присутній
- [ ] Async/await правильно використовується

---

## ?? Файли для створення

```
Application/
??? Services/
    ??? IApplicationService.cs
    ??? IUserService.cs
    ??? UserService.cs
    ??? ICustomerService.cs
    ??? CustomerService.cs
    ??? IOrderService.cs
    ??? OrderService.cs
    ??? ICourierService.cs
    ??? CourierService.cs
    ??? ITariffService.cs
    ??? TariffService.cs
    ??? IPaymentService.cs
    ??? PaymentService.cs
    ??? INotificationService.cs
    ??? NotificationService.cs
    ??? IOrderPricingService.cs
    ??? OrderPricingService.cs
    ??? IOrderMatchingService.cs
    ??? OrderMatchingService.cs
    ??? ILocationService.cs
    ??? LocationService.cs
    ??? IFileStorageService.cs
    ??? FileStorageService.cs

OwnDeliveryApiP33.Tests.Unit/
??? Application/
    ??? Services/
        ??? CustomerServiceTests.cs
        ??? OrderServiceTests.cs
        ??? CourierServiceTests.cs
        ??? TariffServiceTests.cs
        ??? PaymentServiceTests.cs
        ??? NotificationServiceTests.cs
```

---

## ?? Примітки для розробника

1. **Dependency Injection:**
   - Усі dependencies через конструктор
   - Використовуйте interface injection

2. **Error Handling:**
   ```csharp
   if (customer == null)
       throw new EntityNotFoundException($"Customer with ID {customerId} not found");
   ```

3. **Logging:**
   ```csharp
   _logger.LogInformation("Creating order for customer {CustomerId}", customerId);
   _logger.LogError("Failed to create order: {Error}", ex.Message);
   ```

4. **Mapper Usage:**
   ```csharp
   var response = _mapper.Map<OrderResponse>(order);
   ```

5. **Transaction Management:**
   ```csharp
   await _unitOfWork.BeginTransactionAsync();
   try 
   {
       // операції
       await _unitOfWork.CommitTransactionAsync();
   }
   catch
   {
       await _unitOfWork.RollbackTransactionAsync();
   }
   ```

---

**Крок 4 з 12**  
**Статус:** ?? Готово до виконання  
**Очікуваний час:** 8-10 годин
