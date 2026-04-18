using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Extensions;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

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

    /// <summary>Get the profile of the currently authenticated courier</summary>
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

    /// <summary>Get courier by ID</summary>
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
