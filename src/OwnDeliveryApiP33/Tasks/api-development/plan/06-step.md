# ?? Step 06: API Controllers & Route Configuration

## ?? Мета
Реалізувати RESTful API controllers для всіх основних ресурсів з правильною маршрутизацією та дозволами.

## ?? Описання
Цей крок створює контролери з HTTP endpoints, налаштовує маршрути, додає авторизацію та документацію через Swagger.

---

## ? Вимоги до виконання

### 1. **Auth Controller** (оновити існуючий)

#### a. **AuthController.cs**
```csharp
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    /// <summary>
    /// Register a new customer account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> RegisterCustomer([FromBody] RegisterCustomerRequest request)
    {
        var result = await _authService.RegisterCustomerAsync(request);
        return CreatedAtAction(nameof(RegisterCustomer), result);
    }
    
    /// <summary>
    /// Register a new courier account
    /// </summary>
    [HttpPost("register-courier")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> RegisterCourier([FromBody] RegisterCourierRequest request)
    {
        var result = await _authService.RegisterCourierAsync(request);
        return CreatedAtAction(nameof(RegisterCourier), result);
    }
    
    /// <summary>
    /// Login to the system
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
    
    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }
    
    /// <summary>
    /// Logout from the system
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.GetUserId(); // Extension method
        await _authService.LogoutAsync(userId);
        return Ok();
    }
    
    /// <summary>
    /// Verify email address
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        await _authService.VerifyEmailAsync(token);
        return Ok();
    }
    
    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.RequestPasswordResetAsync(request.Email);
        return Ok();
    }
    
    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
        return Ok();
    }
}
```

### 2. **Customers Controller** (новий)

#### a. **CustomersController.cs**
```csharp
[ApiController]
[Route("api/v1/customers")]
[Authorize(Roles = "Customer")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;
    
    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get customer profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerResponse>> GetProfile()
    {
        var userId = User.GetUserId();
        var result = await _customerService.GetProfileAsync(userId);
        return Ok(result);
    }
    
    /// <summary>
    /// Update customer profile
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerResponse>> UpdateProfile([FromBody] UpdateCustomerProfileRequest request)
    {
        var userId = User.GetUserId();
        var result = await _customerService.UpdateProfileAsync(userId, request);
        return Ok(result);
    }
    
    /// <summary>
    /// Upload customer avatar
    /// </summary>
    [HttpPost("avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
    {
        var userId = User.GetUserId();
        // Implementation in service
        return Ok();
    }
    
    /// <summary>
    /// Get saved addresses
    /// </summary>
    [HttpGet("addresses")]
    [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetSavedAddresses()
    {
        var userId = User.GetUserId();
        var result = await _customerService.GetSavedAddressesAsync(userId);
        return Ok(result);
    }
    
    /// <summary>
    /// Add saved address
    /// </summary>
    [HttpPost("addresses")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAddress([FromBody] AddressDto address)
    {
        var userId = User.GetUserId();
        await _customerService.AddSavedAddressAsync(userId, address);
        return StatusCode(StatusCodes.Status201Created);
    }
    
    /// <summary>
    /// Get customer statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CustomerStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerStatsResponse>> GetStats()
    {
        var userId = User.GetUserId();
        var result = await _customerService.GetStatsAsync(userId);
        return Ok(result);
    }
}
```

### 3. **Orders Controller** (новий)

#### a. **OrdersController.cs**
```csharp
[ApiController]
[Route("api/v1/orders")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;
    
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var customerId = User.GetUserId();
        var result = await _orderService.CreateOrderAsync(customerId, request);
        return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
    }
    
    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        var result = await _orderService.GetOrderAsync(id);
        return Ok(result);
    }
    
    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderResponse>> GetOrderByNumber(string orderNumber)
    {
        var result = await _orderService.GetOrderByNumberAsync(orderNumber);
        return Ok(result);
    }
    
    /// <summary>
    /// Get customer's orders
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetMyOrders([FromQuery] OrderFilterRequest filter)
    {
        var customerId = User.GetUserId();
        var result = await _orderService.GetCustomerOrdersAsync(customerId, filter);
        return Ok(result);
    }
    
    /// <summary>
    /// Cancel order
    /// </summary>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        await _orderService.CancelOrderAsync(id, request.Reason);
        return Ok();
    }
    
    /// <summary>
    /// Rate order and courier
    /// </summary>
    [HttpPost("{id}/rate")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RateOrder(Guid id, [FromBody] RateOrderRequest request)
    {
        var customerId = User.GetUserId();
        await _orderService.RateOrderAsync(id, customerId, request);
        return Ok();
    }
    
    /// <summary>
    /// Get available orders for courier
    /// </summary>
    [HttpGet("available")]
    [Authorize(Roles = "Courier")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAvailableOrders([FromQuery] OrderFilterRequest filter)
    {
        var result = await _orderService.GetAvailableOrdersAsync(filter);
        return Ok(result);
    }
    
    /// <summary>
    /// Accept order by courier
    /// </summary>
    [HttpPost("{id}/accept")]
    [Authorize(Roles = "Courier")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptOrder(Guid id)
    {
        var courierId = User.GetUserId();
        await _orderService.AcceptOrderAsync(id, courierId);
        return Ok();
    }
    
    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Courier,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromForm] OrderStatusUpdateRequest request)
    {
        await _orderService.UpdateOrderStatusAsync(id, request);
        return Ok();
    }
    
    /// <summary>
    /// Get order status history
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(IEnumerable<OrderStatusHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderStatusHistoryResponse>>> GetOrderHistory(Guid id)
    {
        var result = await _orderService.GetOrderHistoryAsync(id);
        return Ok(result);
    }
}
```

### 4. **Couriers Controller** (оновити існуючий)

#### a. **CouriersController.cs**
```csharp
[ApiController]
[Route("api/v1/couriers")]
[Authorize(Roles = "Courier")]
[Produces("application/json")]
public class CouriersController : ControllerBase
{
    private readonly ICourierService _courierService;
    private readonly ILogger<CouriersController> _logger;
    
    public CouriersController(ICourierService courierService, ILogger<CouriersController> logger)
    {
        _courierService = courierService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get courier profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(CourierProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourierProfileResponse>> GetProfile()
    {
        var userId = User.GetUserId();
        var result = await _courierService.GetProfileAsync(userId);
        return Ok(result);
    }
    
    /// <summary>
    /// Update courier profile
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(CourierProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourierProfileResponse>> UpdateProfile([FromBody] UpdateCourierProfileRequest request)
    {
        var userId = User.GetUserId();
        var result = await _courierService.UpdateProfileAsync(userId, request);
        return Ok(result);
    }
    
    /// <summary>
    /// Update courier location
    /// </summary>
    [HttpPost("location")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateRequest request)
    {
        var courierId = User.GetUserId();
        await _courierService.UpdateLocationAsync(courierId, request);
        return Ok();
    }
    
    /// <summary>
    /// Get current location
    /// </summary>
    [HttpGet("location")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LocationResponse>> GetCurrentLocation()
    {
        var courierId = User.GetUserId();
        var result = await _courierService.GetCurrentLocationAsync(courierId);
        return Ok(result);
    }
    
    /// <summary>
    /// Update courier status
    /// </summary>
    [HttpPut("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus([FromBody] CourierStatusUpdateRequest request)
    {
        var courierId = User.GetUserId();
        await _courierService.UpdateStatusAsync(courierId, request);
        return Ok();
    }
    
    /// <summary>
    /// Upload courier document
    /// </summary>
    [HttpPost("documents")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        var courierId = User.GetUserId();
        await _courierService.UploadDocumentAsync(courierId, request);
        return StatusCode(StatusCodes.Status201Created);
    }
    
    /// <summary>
    /// Get courier documents
    /// </summary>
    [HttpGet("documents")]
    [ProducesResponseType(typeof(IEnumerable<CourierDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourierDocumentResponse>>> GetDocuments()
    {
        var courierId = User.GetUserId();
        var result = await _courierService.GetDocumentsAsync(courierId);
        return Ok(result);
    }
    
    /// <summary>
    /// Get courier statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CourierStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourierStatsResponse>> GetStats()
    {
        var courierId = User.GetUserId();
        var result = await _courierService.GetStatsAsync(courierId);
        return Ok(result);
    }
    
    /// <summary>
    /// Get courier ratings
    /// </summary>
    [HttpGet("ratings")]
    [ProducesResponseType(typeof(IEnumerable<CourierRatingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourierRatingResponse>>> GetRatings()
    {
        var courierId = User.GetUserId();
        var result = await _courierService.GetRatingsAsync(courierId);
        return Ok(result);
    }
    
    /// <summary>
    /// Get courier earnings
    /// </summary>
    [HttpGet("earnings")]
    [ProducesResponseType(typeof(CourierEarningsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourierEarningsResponse>> GetEarnings([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var courierId = User.GetUserId();
        var result = await _courierService.GetEarningsAsync(courierId, from, to);
        return Ok(result);
    }
}
```

### 5. **Tariffs Controller** (новий)

#### a. **TariffsController.cs**
```csharp
[ApiController]
[Route("api/v1/tariffs")]
[Produces("application/json")]
public class TariffsController : ControllerBase
{
    private readonly ITariffService _tariffService;
    
    public TariffsController(ITariffService tariffService)
    {
        _tariffService = tariffService;
    }
    
    /// <summary>
    /// Get all active tariffs
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<TariffResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TariffResponse>>> GetActiveTariffs()
    {
        var result = await _tariffService.GetActiveTariffsAsync();
        return Ok(result);
    }
    
    /// <summary>
    /// Calculate delivery cost
    /// </summary>
    [HttpPost("calculate")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculateCost([FromBody] CalculateCostRequest request)
    {
        var result = await _tariffService.CalculateCostAsync(request);
        return Ok(result);
    }
    
    /// <summary>
    /// Create new tariff (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<TariffResponse>> CreateTariff([FromBody] CreateTariffRequest request)
    {
        var result = await _tariffService.CreateTariffAsync(request);
        return CreatedAtAction(nameof(GetTariffById), new { id = result.Id }, result);
    }
    
    /// <summary>
    /// Get tariff by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TariffResponse>> GetTariffById(Guid id)
    {
        var result = await _tariffService.GetTariffByIdAsync(id);
        return Ok(result);
    }
    
    /// <summary>
    /// Update tariff (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TariffResponse>> UpdateTariff(Guid id, [FromBody] UpdateTariffRequest request)
    {
        var result = await _tariffService.UpdateTariffAsync(id, request);
        return Ok(result);
    }
    
    /// <summary>
    /// Delete tariff (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTariff(Guid id)
    {
        await _tariffService.DeleteTariffAsync(id);
        return NoContent();
    }
}
```

### 6. **Admin Controller** (новий)

#### a. **AdminController.cs**
```csharp
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly ILogger<AdminController> _logger;
    
    public AdminController(IOrderService orderService, IUserService userService, ILogger<AdminController> logger)
    {
        _orderService = orderService;
        _userService = userService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all orders (with filtering)
    /// </summary>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders([FromQuery] AdminOrderFilterRequest filter)
    {
        var result = await _orderService.GetAllOrdersAsync(filter);
        return Ok(result);
    }
    
    /// <summary>
    /// Assign courier to order
    /// </summary>
    [HttpPut("orders/{orderId}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignCourier(Guid orderId, [FromBody] AssignCourierRequest request)
    {
        await _orderService.AssignCourierAsync(orderId, request.CourierId);
        return Ok();
    }
    
    /// <summary>
    /// Get order analytics
    /// </summary>
    [HttpGet("analytics/orders")]
    [ProducesResponseType(typeof(OrderAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderAnalyticsResponse>> GetOrderAnalytics([FromQuery] AnalyticsFilterRequest filter)
    {
        var result = await _orderService.GetAnalyticsAsync(filter);
        return Ok(result);
    }
    
    /// <summary>
    /// Block user
    /// </summary>
    [HttpPut("users/{userId}/block")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BlockUser(Guid userId, [FromBody] BlockUserRequest request)
    {
        await _userService.BlockUserAsync(userId, request.Reason);
        return Ok();
    }
    
    /// <summary>
    /// Get all users by role
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsersByRole([FromQuery] UserRole? role, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        if (role == null)
        {
            return BadRequest("Role parameter is required");
        }
        
        var result = await _userService.GetUsersByRoleAsync(role.Value, skip, take);
        return Ok(result);
    }
}
```

### 7. **Global Exception Handler Middleware** (новий)

#### a. **ExceptionHandlingMiddleware.cs**
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();
        
        switch (exception)
        {
            case EntityNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = exception.Message;
                break;
            case ValidationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = exception.Message;
                break;
            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized";
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "An error occurred";
                break;
        }
        
        return context.Response.WriteAsJsonAsync(response);
    }
}

public class ErrorResponse
{
    public string Message { get; set; }
    public int? Code { get; set; }
}
```

### 8. **Register in Program.cs**

```csharp
// Додати в Program.cs:
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OwnDelivery API",
        Version = "v1",
        Description = "API for Own Delivery Service"
    });
    
    // JWT scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
```

---

## ?? Unit Tests

Створіть tests у `OwnDeliveryApiP33.Tests.Unit/Controllers/`:

### 1. **Auth Controller Tests**
- ? RegisterCustomer creates account
- ? Login returns token
- ? Invalid credentials fail
- ? Logout works

### 2. **Order Controller Tests**
- ? CreateOrder creates order with correct data
- ? GetOrder returns order
- ? GetMyOrders returns user's orders
- ? CancelOrder cancels correctly
- ? RateOrder saves rating

### 3. **Courier Controller Tests**
- ? UpdateLocation saves location
- ? UpdateStatus changes status
- ? UploadDocument saves document

---

## ?? Очікувані результати

1. ? Всі controllers створені
2. ? Всі endpoints реалізовані з правильними HTTP методами
3. ? Авторизація налаштована на контролерах
4. ? Swagger документація доступна
5. ? Global exception handling налаштований
6. ? Unit test покриття >75%

---

## ?? Критерії приймання

- [ ] Усі controllers створені
- [ ] Усі endpoints мають правильні HTTP методи
- [ ] Авторизація на місці
- [ ] Swagger документація повна
- [ ] Exception handling налаштований
- [ ] Unit test покриття >75%

---

**Крок 6 з 12**  
**Статус:** ?? Готово до виконання  
**Очікуваний час:** 8-10 годин
