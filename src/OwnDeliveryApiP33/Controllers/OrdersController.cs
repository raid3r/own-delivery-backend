using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Extensions;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

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

    /// <summary>Create new order</summary>
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

    /// <summary>Get order by ID</summary>
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

    /// <summary>Get order by order number</summary>
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

    /// <summary>Get my orders (customer)</summary>
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

    /// <summary>Get orders for courier</summary>
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

    /// <summary>Update order status</summary>
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

    /// <summary>Cancel order</summary>
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

    /// <summary>Rate order</summary>
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

/// <summary>Request to cancel an order</summary>
public record CancelOrderRequest(string Reason);
