using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers.Admin;

/// <summary>
/// Provides administrator endpoints for tariff management.
/// </summary>
[ApiController]
[Route("api/v1/admin/tariffs")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class AdminTariffsController : ControllerBase
{
    private readonly ITariffService _tariffService;
    private readonly ILogger<AdminTariffsController> _logger;

    public AdminTariffsController(
        ITariffService tariffService,
        ILogger<AdminTariffsController> logger)
    {
        _tariffService = tariffService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TariffResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTariffs(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var tariffs = await _tariffService.GetTariffsAsync(skip, take, isActive, ct);
        return Ok(tariffs);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTariff(Guid id, CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.GetTariffAsync(id, ct);
            return Ok(tariff);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTariff([FromBody] CreateTariffRequest request, CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.CreateTariffAsync(request, ct);
            return CreatedAtAction(nameof(GetTariff), new { id = tariff.Id }, tariff);
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tariff from admin API");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TariffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTariff(Guid id, [FromBody] UpdateTariffRequest request, CancellationToken ct)
    {
        try
        {
            var tariff = await _tariffService.UpdateTariffAsync(id, request, ct);
            return Ok(tariff);
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
            _logger.LogError(ex, "Error updating tariff {TariffId} from admin API", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateTariff(Guid id, CancellationToken ct)
    {
        var deactivated = await _tariffService.DeactivateTariffAsync(id, ct);
        if (!deactivated)
            return NotFound(new { message = "Tariff not found" });

        return Ok(new { message = "Tariff deactivated successfully" });
    }
}
