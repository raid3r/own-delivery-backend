using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Extensions;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

/// <summary>
/// Provides courier profile endpoints.
/// </summary>
[ApiController]
[Route("api/v1/couriers")]
[Produces("application/json")]
[Authorize]
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
    /// Returns the profile of the authenticated courier.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Courier profile returned.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Courier profile was not found.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CourierProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        try
        {
            var courierId = User.GetUserId();
            var profile = await _courierService.GetProfileAsync(courierId, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns a courier profile by courier identifier.
    /// </summary>
    /// <param name="id">Courier identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Courier profile returned.</response>
    /// <response code="404">Courier profile was not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CourierProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourierProfileResponse>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var profile = await _courierService.GetProfileAsync(id, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
