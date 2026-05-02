using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Application.Extensions;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

/// <summary>
/// Provides endpoints for creating and managing delivery orders.
/// </summary>
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
    /// Creates a new order for the authenticated customer.
    /// </summary>
    /// <remarks>
    /// The caller's user ID is read from the JWT token — no need to pass customer ID in the body.
    /// Delivery cost is automatically computed from the selected tariff, weight and distance.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/orders
    ///     {
    ///         "pickupAddress":   { "city": "Kyiv",  "street": "Khreshchatyk",  "buildingNumber": "1",  "postalCode": "01001", "latitude": 50.4501, "longitude": 30.5234 },
    ///         "deliveryAddress": { "city": "Kyiv",  "street": "Bohdana Khmelnytskoho", "buildingNumber": "5", "postalCode": "01030", "latitude": 50.4456, "longitude": 30.5219 },
    ///         "weight": 2.5,
    ///         "dimensions": { "width": 20, "length": 30, "height": 15 },
    ///         "tariffId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "description": "Fragile goods"
    ///     }
    /// </remarks>
    /// <param name="request">Order creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Order created successfully.</response>
    /// <response code="400">Request is invalid.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        try
        {
            var customerId = User.GetUserId();
            var order = await _orderService.CreateOrderAsync(customerId, request, ct);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns an order by identifier.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order returned.</response>
    /// <response code="404">Order was not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id, ct);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns an order by business order number.
    /// </summary>
    /// <param name="orderNumber">Order number.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order returned.</response>
    /// <response code="404">Order was not found.</response>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken ct)
    {
        try
        {
            var order = await _orderService.GetByOrderNumberAsync(orderNumber, ct);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns paged orders for the authenticated customer.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Orders returned.</response>
    /// <response code="400">Request is invalid.</response>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        try
        {
            var customerId = User.GetUserId();
            var orders = await _orderService.GetCustomerOrdersAsync(customerId, skip, take, ct);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns a paged list of available (unassigned) orders for couriers.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to return (max 100).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Available orders returned.</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(PagedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableOrders([FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        try
        {
            var response = await _orderService.GetAvailableOrdersAsync(skip, take, ct);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available orders");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Accepts an available order: assigns the authenticated courier and transitions status to Accepted.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order accepted successfully.</response>
    /// <response code="409">Order is already assigned to another courier.</response>
    /// <response code="404">Order was not found.</response>
    [HttpPost("{id}/accept")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptOrder(Guid id, CancellationToken ct)
    {
        try
        {
            var courierId = User.GetUserId();
            var order = await _orderService.AcceptOrderAsync(id, courierId, ct);
            return Ok(order);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting order {OrderId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns paged orders assigned to a specific courier.
    /// </summary>
    /// <param name="courierId">Courier identifier.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Orders returned.</response>
    /// <response code="400">Request is invalid.</response>
    [HttpGet("courier/{courierId}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourierOrders(Guid courierId, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        try
        {
            var orders = await _orderService.GetCourierOrdersAsync(courierId, skip, take, ct);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courier orders");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the status of an order.
    /// </summary>
    /// <remarks>
    /// Allowed status transitions depend on the current state of the order.
    /// Typical flow: <c>Pending</c> → <c>Assigned</c> → <c>PickedUp</c> → <c>InTransit</c> → <c>Delivered</c>.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/v1/orders/{id}/status
    ///     {
    ///         "newStatus": "PickedUp",
    ///         "location": { "latitude": 50.4501, "longitude": 30.5234 }
    ///     }
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Status update payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order status updated.</response>
    /// <response code="400">Request is invalid.</response>
    /// <response code="404">Order was not found.</response>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatusUpdateRequest request, CancellationToken ct)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request, ct);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancels an order with a specified reason.
    /// </summary>
    /// <remarks>
    /// Only orders in <c>Pending</c> or <c>Assigned</c> status can be cancelled.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/orders/{id}/cancel
    ///     {
    ///         "reason": "Customer changed delivery address"
    ///     }
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Cancellation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order cancelled successfully.</response>
    /// <response code="400">Request is invalid.</response>
    /// <response code="404">Order was not found.</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        try
        {
            await _orderService.CancelOrderAsync(id, request.Reason, ct);
            return Ok(new { message = "Order cancelled successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submits a rating for a completed order.
    /// </summary>
    /// <remarks>
    /// Rating is allowed only for orders with <c>Delivered</c> status.
    /// Score must be in range 1–5.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/orders/{id}/rate
    ///     {
    ///         "score": 5,
    ///         "comment": "Very fast delivery, highly recommended!"
    ///     }
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Rating payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order rated successfully.</response>
    /// <response code="400">Request is invalid.</response>
    /// <response code="404">Order was not found.</response>
    [HttpPost("{id}/rate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RateOrder(Guid id, [FromBody] RateOrderRequest request, CancellationToken ct)
    {
        try
        {
            await _orderService.RateOrderAsync(id, request, ct);
            return Ok(new { message = "Order rated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Request payload for order cancellation.
/// </summary>
/// <param name="Reason">Reason for cancellation shown in order history.</param>
public record CancelOrderRequest(string Reason);
