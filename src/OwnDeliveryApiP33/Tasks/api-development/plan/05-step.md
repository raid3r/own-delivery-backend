# ?? Step 05: DTOs, Validators & AutoMapper Configuration

## ?? Мета
Створити Data Transfer Objects (DTOs) для безпеки та валідацію input даних через FluentValidation.

## ?? Описання
Цей крок фокусується на створенні DTOs для кожного endpoint, валідаторів та AutoMapper конфігурації для трансформації між entities та DTOs.

---

## ? Вимоги до виконання

### 1. **Request DTOs** (папка: `Application/DTOs/Requests/`)

#### a. **Auth DTOs**
```csharp
// RegisterCustomerRequest.cs
public class RegisterCustomerRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
}

// RegisterCourierRequest.cs
public class RegisterCourierRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string LicenseNumber { get; set; }
}

// LoginRequest.cs
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// RefreshTokenRequest.cs
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}

// ChangePasswordRequest.cs
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}

// ResetPasswordRequest.cs
public class ResetPasswordRequest
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}
```

#### b. **Order DTOs**
```csharp
// CreateOrderRequest.cs
public class CreateOrderRequest
{
    public AddressDto PickupAddress { get; set; }
    public AddressDto DeliveryAddress { get; set; }
    public decimal Weight { get; set; }
    public DimensionsDto Dimensions { get; set; }
    public string Description { get; set; }
    public string? SpecialInstructions { get; set; }
    public Guid TariffId { get; set; }
    public DateTime? ScheduledDeliveryTime { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}

// UpdateOrderRequest.cs
public class UpdateOrderRequest
{
    public string? Description { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTime? ScheduledDeliveryTime { get; set; }
}

// OrderStatusUpdateRequest.cs
public class OrderStatusUpdateRequest
{
    public OrderStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public LocationDto? Location { get; set; }
    public IFormFile? Proof { get; set; } // фото доказу
}

// RateOrderRequest.cs
public class RateOrderRequest
{
    public int Score { get; set; } // 1-5
    public string? Comment { get; set; }
    public RatingType Type { get; set; }
}

// OrderFilterRequest.cs
public class OrderFilterRequest
{
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}
```

#### c. **Courier DTOs**
```csharp
// UpdateCourierProfileRequest.cs
public class UpdateCourierProfileRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? BankAccount { get; set; }
}

// UploadDocumentRequest.cs
public class UploadDocumentRequest
{
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public IFormFile DocumentFile { get; set; }
    public DateTime ExpirationDate { get; set; }
}

// LocationUpdateRequest.cs
public class LocationUpdateRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Accuracy { get; set; }
    public decimal? Altitude { get; set; }
    public decimal? Speed { get; set; }
}

// CourierStatusUpdateRequest.cs
public class CourierStatusUpdateRequest
{
    public CourierStatus NewStatus { get; set; }
}

// CourierOrderFilterRequest.cs
public class CourierOrderFilterRequest
{
    public CourierOrderStatus? Status { get; set; } // Active, Completed, etc.
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}
```

#### d. **Tariff DTOs**
```csharp
// CreateTariffRequest.cs
public class CreateTariffRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BaseCost { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal PricePerKg { get; set; }
    public int EstimatedDeliveryTimeMinutes { get; set; }
    public decimal MaxWeight { get; set; }
    public DimensionsDto MaxDimensions { get; set; }
}

// UpdateTariffRequest.cs
public class UpdateTariffRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BaseCost { get; set; }
    public decimal? PricePerKm { get; set; }
    public decimal? PricePerKg { get; set; }
    public bool? IsActive { get; set; }
}

// CalculateCostRequest.cs
public class CalculateCostRequest
{
    public decimal Distance { get; set; }
    public decimal Weight { get; set; }
    public Guid TariffId { get; set; }
}
```

#### e. **Payment DTOs**
```csharp
// CreatePaymentRequest.cs
public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
}

// ProcessPaymentRequest.cs
public class ProcessPaymentRequest
{
    public string ProviderTransactionId { get; set; }
    public Dictionary<string, string>? MetaData { get; set; }
}

// RefundPaymentRequest.cs
public class RefundPaymentRequest
{
    public string Reason { get; set; }
}
```

#### f. **User DTOs**
```csharp
// UpdateProfileRequest.cs
public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
}

// UpdateCustomerProfileRequest.cs
public class UpdateCustomerProfileRequest : UpdateProfileRequest
{
    public string? PreferredDeliveryAddress { get; set; }
}
```

#### g. **Common DTOs**
```csharp
// AddressDto.cs
public class AddressDto
{
    public string City { get; set; }
    public string Street { get; set; }
    public string BuildingNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public string PostalCode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Description { get; set; }
}

// LocationDto.cs
public class LocationDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

// DimensionsDto.cs
public class DimensionsDto
{
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal Height { get; set; }
}

// NotificationRequest.cs
public class NotificationRequest
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

### 2. **Response DTOs** (папка: `Application/DTOs/Responses/`)

#### a. **Auth DTOs**
```csharp
// AuthResponse.cs
public class AuthResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

#### b. **User DTOs**
```csharp
// UserResponse.cs
public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// CustomerResponse.cs
public class CustomerResponse : UserResponse
{
    public decimal AverageRating { get; set; }
    public int TotalOrders { get; set; }
    public int SuccessfulOrders { get; set; }
    public int CancelledOrders { get; set; }
}

// CourierProfileResponse.cs
public class CourierProfileResponse : UserResponse
{
    public string LicenseNumber { get; set; }
    public bool IsVerified { get; set; }
    public CourierStatus CurrentStatus { get; set; }
    public LocationDto? CurrentLocation { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalDeliveries { get; set; }
    public int CompletedDeliveries { get; set; }
    public decimal TotalEarnings { get; set; }
}
```

#### c. **Order DTOs**
```csharp
// OrderResponse.cs
public class OrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? CourierId { get; set; }
    public OrderStatus Status { get; set; }
    public AddressDto PickupAddress { get; set; }
    public AddressDto DeliveryAddress { get; set; }
    public decimal Weight { get; set; }
    public DimensionsDto Dimensions { get; set; }
    public string Description { get; set; }
    public decimal Cost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }
}

// OrderStatusHistoryResponse.cs
public class OrderStatusHistoryResponse
{
    public Guid Id { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ProofUrl { get; set; }
}
```

#### d. **Tariff DTOs**
```csharp
// TariffResponse.cs
public class TariffResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BaseCost { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal PricePerKg { get; set; }
    public TimeSpan EstimatedDeliveryTime { get; set; }
    public decimal MaxWeight { get; set; }
    public bool IsActive { get; set; }
}
```

#### e. **Courier DTOs**
```csharp
// CourierDocumentResponse.cs
public class CourierDocumentResponse
{
    public Guid Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public string DocumentUrl { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DocumentStatus Status { get; set; }
    public DateTime UploadedAt { get; set; }
}

// CourierStatsResponse.cs
public class CourierStatsResponse
{
    public int TotalDeliveries { get; set; }
    public int CompletedDeliveries { get; set; }
    public int CancelledDeliveries { get; set; }
    public decimal AverageRating { get; set; }
    public TimeSpan AverageDeliveryTime { get; set; }
    public decimal TotalEarnings { get; set; }
    public double CompletionRate { get; set; }
}

// CourierRatingResponse.cs
public class CourierRatingResponse
{
    public Guid Id { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public RatingType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### f. **Payment DTOs**
```csharp
// PaymentResponse.cs
public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

#### g. **Notification DTOs**
```csharp
// NotificationResponse.cs
public class NotificationResponse
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
```

---

### 3. **Validators** (папка: `Application/Validators/`)

#### a. **Auth Validators**
```csharp
// RegisterCustomerRequestValidator.cs
public class RegisterCustomerRequestValidator : AbstractValidator<RegisterCustomerRequest>
{
    public RegisterCustomerRequestValidator()
    {\n        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase")
            .Matches(@"[0-9]").WithMessage("Password must contain number");
        
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(2).MaximumLength(100);
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[0-9]{10,}$").WithMessage("Invalid phone number format");
    }
}

// LoginRequestValidator.cs
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8);
    }
}

// RegisterCourierRequestValidator.cs
public class RegisterCourierRequestValidator : AbstractValidator<RegisterCourierRequest>
{
    public RegisterCourierRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[0-9]{10,}$");
        RuleFor(x => x.LicenseNumber).NotEmpty().WithMessage("License number is required");
    }
}
```

#### b. **Order Validators**
```csharp
// CreateOrderRequestValidator.cs
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.PickupAddress).SetValidator(new AddressDtoValidator());
        RuleFor(x => x.DeliveryAddress).SetValidator(new AddressDtoValidator());
        
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Weight must be less than 1000 kg");
        
        RuleFor(x => x.Dimensions).SetValidator(new DimensionsDtoValidator());
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500);
        
        RuleFor(x => x.TariffId).NotEmpty().WithMessage("Tariff must be selected");
    }
}

// OrderStatusUpdateRequestValidator.cs
public class OrderStatusUpdateRequestValidator : AbstractValidator<OrderStatusUpdateRequest>
{
    public OrderStatusUpdateRequestValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

// RateOrderRequestValidator.cs
public class RateOrderRequestValidator : AbstractValidator<RateOrderRequest>
{
    public RateOrderRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Score must be between 1 and 5");
        
        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Comment must be less than 500 characters");
    }
}
```

#### c. **Common Validators**
```csharp
// AddressDtoValidator.cs
public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.City).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Street).NotEmpty().MinimumLength(2);
        RuleFor(x => x.BuildingNumber).NotEmpty();
        RuleFor(x => x.PostalCode).NotEmpty();
        
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Invalid latitude");
        
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Invalid longitude");
    }
}

// DimensionsDtoValidator.cs
public class DimensionsDtoValidator : AbstractValidator<DimensionsDto>
{
    public DimensionsDtoValidator()
    {
        RuleFor(x => x.Width).GreaterThan(0);
        RuleFor(x => x.Length).GreaterThan(0);
        RuleFor(x => x.Height).GreaterThan(0);
    }
}
```

---

### 4. **AutoMapper Configuration** (папка: `Application/Mapping/`)

#### a. **MappingProfile.cs**
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserResponse>().ReverseMap();
        CreateMap<Customer, CustomerResponse>();
        CreateMap<Courier, CourierProfileResponse>();
        
        // Order mappings
        CreateMap<Order, OrderResponse>();
        CreateMap<OrderStatusHistory, OrderStatusHistoryResponse>();
        CreateMap<CreateOrderRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        
        // Address mappings
        CreateMap<Address, AddressDto>().ReverseMap();
        
        // Location mappings
        CreateMap<Location, LocationDto>().ReverseMap();
        
        // Dimensions mappings
        CreateMap<Dimensions, DimensionsDto>().ReverseMap();
        
        // Tariff mappings
        CreateMap<Tariff, TariffResponse>();
        CreateMap<CreateTariffRequest, Tariff>()
            .ForMember(dest => dest.EstimatedDeliveryTime, 
                opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.EstimatedDeliveryTimeMinutes)))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        
        // Rating mappings
        CreateMap<Rating, CourierRatingResponse>();
        
        // Payment mappings
        CreateMap<Payment, PaymentResponse>();
        
        // Notification mappings
        CreateMap<Notification, NotificationResponse>();
        
        // Document mappings
        CreateMap<CourierDocument, CourierDocumentResponse>();
    }
}
```

### 5. **Register in DI** (Program.cs)

```csharp
// Додати в Program.cs:
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining(typeof(RegisterCustomerRequestValidator));
```

---

## ?? Unit Tests

Створіть tests у `OwnDeliveryApiP33.Tests.Unit/Application/`:

### 1. **Validator Tests**
- ? RegisterCustomerRequestValidator validates correctly
- ? Invalid email fails validation
- ? Weak password fails validation
- ? Invalid phone number fails validation
- ? AddressDtoValidator validates coordinates

### 2. **Mapper Tests**
- ? User maps to UserResponse correctly
- ? CreateOrderRequest maps to Order
- ? Address maps to AddressDto bidirectionally
- ? Null values handled correctly

---

## ?? Очікувані результати

1. ? Всі Request DTOs створені
2. ? Всі Response DTOs створені
3. ? Всі Validators створені та зареєстровані
4. ? AutoMapper профіл налаштований
5. ? DI налаштований в Program.cs
6. ? Unit test покриття >80%

---

## ?? Критерії приймання

- [ ] Усі DTOs створені коректно
- [ ] Усі Validators покривають вимоги
- [ ] AutoMapper конфігурація повна
- [ ] DI правильно налаштована
- [ ] Unit test покриття >80%
- [ ] Немає компіляційних помилок

---

**Крок 5 з 12**  
**Статус:** ?? Готово до виконання  
**Очікуваний час:** 8-10 годин
