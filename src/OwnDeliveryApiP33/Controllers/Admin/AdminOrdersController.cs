using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Controllers.Admin;

/// <summary>
/// Provides administrator endpoints for order management.
/// </summary>
[ApiController]
[Route("api/v1/admin/orders")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<AdminOrdersController> _logger;

    public AdminOrdersController(
        IOrderService orderService,
        ILogger<AdminOrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? courierId = null,
        CancellationToken ct = default)
    {
        var orders = await _orderService.GetOrdersAsync(skip, take, status, customerId, courierId, ct);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id, ct);
            return Ok(order);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder([FromBody] AdminCreateOrderRequest request, CancellationToken ct)
    {
        try
        {
            var order = await _orderService.AdminCreateOrderAsync(request, ct);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
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
            _logger.LogError(ex, "Error creating order from admin API");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] OrderStatusUpdateRequest request,
        CancellationToken ct)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request, ct);
            return Ok(order);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] AdminCancelOrderRequest request,
        CancellationToken ct)
    {
        try
        {
            var cancelled = await _orderService.CancelOrderAsync(id, request.Reason, ct);
            if (!cancelled)
                return NotFound(new { message = "Order not found" });

            return Ok(new { message = "Order cancelled successfully" });
        }
        catch (OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId} from admin API", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
