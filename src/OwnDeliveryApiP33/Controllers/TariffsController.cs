using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

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

    /// <summary>Get all active tariffs</summary>
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

    /// <summary>Get tariff by ID</summary>
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

    /// <summary>Get tariff by name</summary>
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

    /// <summary>Get default tariff</summary>
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

    /// <summary>Create new tariff (admin only)</summary>
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

    /// <summary>Update tariff (admin only)</summary>
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

    /// <summary>Deactivate tariff (admin only)</summary>
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
