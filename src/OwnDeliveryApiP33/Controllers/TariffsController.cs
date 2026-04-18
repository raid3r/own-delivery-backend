using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

/// <summary>
/// Provides endpoints to view and manage delivery tariffs.
/// </summary>
[ApiController]
[Route("api/v1/tariffs")]
[Produces("application/json")]
public class TariffsController : ControllerBase
{
    private readonly ITariffService _tariffService;
    private readonly ILogger<TariffsController> _logger;

    public TariffsController(ITariffService tariffService, ILogger<TariffsController> logger)
    {
        _tariffService = tariffService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all active tariffs available for order pricing.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Active tariffs returned.</response>
    /// <response code="400">Request cannot be processed.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<TariffResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        try
        {
            var tariffs = await _tariffService.GetActiveTariffsAsync(ct);
            return Ok(tariffs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active tariffs");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns a tariff by identifier.
    /// </summary>
    /// <param name="id">Tariff identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Tariff returned.</response>
    /// <response code="404">Tariff was not found.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTariff(Guid id, CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.GetTariffAsync(id, ct);
            return Ok(tariff);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns a tariff by unique name.
    /// </summary>
    /// <param name="name">Tariff name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Tariff returned.</response>
    /// <response code="404">Tariff was not found.</response>
    [HttpGet("name/{name}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name, CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.GetByNameAsync(name, ct);
            return Ok(tariff);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns the default tariff used when no tariff is explicitly selected.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Default tariff returned.</response>
    /// <response code="404">Default tariff is not configured.</response>
    [HttpGet("default")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefault(CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.GetDefaultTariffAsync(ct);
            return Ok(tariff);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new tariff. Administrator role is required.
    /// </summary>
    /// <remarks>
    /// Tariff names must be unique. BaseCost, PricePerKm and PricePerKg must be non-negative.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/tariffs
    ///     {
    ///         "name": "Standard",
    ///         "baseCost": 50.00,
    ///         "pricePerKm": 5.00,
    ///         "pricePerKg": 2.00,
    ///         "estimatedDeliveryTime": 60,
    ///         "maxWeight": 30.0,
    ///         "maxDimensions": { "width": 60, "length": 60, "height": 60 },
    ///         "description": "Standard same-day delivery"
    ///     }
    /// </remarks>
    /// <param name="request">Tariff creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Tariff created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have administrator role.</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTariff([FromBody] CreateTariffRequest request, CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.CreateTariffAsync(request, ct);
            return CreatedAtAction(nameof(GetTariff), new { id = tariff.Id }, tariff);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tariff");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing tariff. Administrator role is required.
    /// </summary>
    /// <remarks>
    /// All fields are optional — only provided fields will be updated (patch semantics).
    ///
    /// Sample request:
    ///
    ///     PUT /api/v1/tariffs/{id}
    ///     {
    ///         "pricePerKm": 6.50,
    ///         "isActive": true
    ///     }
    /// </remarks>
    /// <param name="id">Tariff identifier.</param>
    /// <param name="request">Tariff update payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Tariff updated successfully.</response>
    /// <response code="400">Request is invalid.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have administrator role.</response>
    /// <response code="404">Tariff was not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTariff(Guid id, [FromBody] UpdateTariffRequest request, CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.UpdateTariffAsync(id, request, ct);
            return Ok(tariff);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivates an existing tariff. Administrator role is required.
    /// </summary>
    /// <param name="id">Tariff identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Tariff deactivated successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have administrator role.</response>
    /// <response code="404">Tariff was not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateTariff(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _tariffService.DeactivateTariffAsync(id, ct);
            if (!result)
                return NotFound(new { message = "Tariff not found" });

            return Ok(new { message = "Tariff deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating tariff");
            return BadRequest(new { message = ex.Message });
        }
    }
}
