using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers.Admin;

/// <summary>
/// Provides administrator endpoints for customer management.
/// </summary>
[ApiController]
[Route("api/v1/admin/customers")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class AdminCustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<AdminCustomersController> _logger;

    public AdminCustomersController(
        ICustomerService customerService,
        ILogger<AdminCustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        var customers = await _customerService.GetCustomersAsync(skip, take, ct);
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken ct)
    {
        try
        {
            var customer = await _customerService.GetProfileAsync(id, ct);
            return Ok(customer);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCustomer([FromBody] AdminCreateCustomerRequest request, CancellationToken ct)
    {
        try
        {
            var customer = await _customerService.CreateCustomerAsync(request, ct);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer from admin API");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] AdminUpdateCustomerRequest request,
        CancellationToken ct)
    {
        try
        {
            var customer = await _customerService.UpdateCustomerAsync(id, request, ct);
            return Ok(customer);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId} from admin API", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken ct)
    {
        var deleted = await _customerService.DeleteCustomerAsync(id, ct);
        if (!deleted)
            return NotFound(new { message = "Customer not found" });

        return Ok(new { message = "Customer deleted successfully" });
    }
}
