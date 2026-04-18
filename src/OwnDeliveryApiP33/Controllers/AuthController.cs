using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Extensions;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

/// <summary>
/// Provides authentication endpoints for courier accounts.
/// </summary>
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
    /// Registers a new courier account and returns an access token.
    /// </summary>
    /// <remarks>
    /// Creates a courier account, hashes the password and returns a signed JWT access token.
    /// Email is stored in lower case; duplicate emails (case-insensitive) are rejected with 409.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/auth/register
    ///     {
    ///         "firstName": "Olga",
    ///         "lastName": "Kovalenko",
    ///         "email": "olga@example.com",
    ///         "password": "SecretPass1",
    ///         "phoneNumber": "+380501234567"
    ///     }
    /// </remarks>
    /// <param name="request">Courier registration payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Courier registered successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="409">A courier with the same email already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCourierRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, ct);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a courier and returns an access token.
    /// </summary>
    /// <remarks>
    /// Email matching is case-insensitive. Returns a signed JWT token valid for 15 minutes.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/auth/login
    ///     {
    ///         "email": "olga@example.com",
    ///         "password": "SecretPass1"
    ///     }
    /// </remarks>
    /// <param name="request">Courier login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCourierRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user using the generic login contract.
    /// </summary>
    /// <remarks>
    /// Intended for customers or admin users. Works identically to /login but accepts
    /// the shared <c>LoginRequest</c> contract instead of <c>LoginCourierRequest</c>.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/auth/login-generic
    ///     {
    ///         "email": "customer@example.com",
    ///         "password": "SecretPass1"
    ///     }
    /// </remarks>
    /// <param name="request">User login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login-generic")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginGeneric([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logs out the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Invalidates the current refresh token associated with the user session.
    /// The JWT access token remains valid until its natural expiry.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Logout completed.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            await _authService.LogoutAsync(userId, ct);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verifies an email address using a verification token.
    /// </summary>
    /// <param name="token">Email verification token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Email verified successfully.</response>
    /// <response code="400">Token is invalid or expired.</response>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
    {
        try
        {
            await _authService.VerifyEmailAsync(token, ct);
            return Ok(new { message = "Email verified successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Starts the password reset flow for the specified email.
    /// </summary>
    /// <remarks>
    /// Always returns 200 to prevent user enumeration, even if the email is not registered.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/auth/forgot-password
    ///     {
    ///         "email": "olga@example.com"
    ///     }
    /// </remarks>
    /// <param name="request">Password reset request payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Reset instructions accepted.</response>
    /// <response code="400">Request cannot be processed.</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        try
        {
            await _authService.RequestPasswordResetAsync(request.Email, ct);
            return Ok(new { message = "Password reset instructions sent to email" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Resets a password using a reset token.
    /// </summary>
    /// <param name="request">Password reset payload containing token and new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Token is invalid or request is malformed.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        try
        {
            await _authService.ResetPasswordAsync(request.Token, request.NewPassword, ct);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new access token pair.
    /// </summary>
    /// <remarks>
    /// Old refresh token is rotated on success — use the new one returned in the response.
    /// Tokens are valid for 7 days. A rejected token returns 401.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/auth/refresh
    ///     {
    ///         "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
    ///     }
    /// </remarks>
    /// <param name="request">Refresh token payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="400">Refresh token is missing or invalid format.</response>
    /// <response code="401">Refresh token is rejected.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { message = "Refresh token is required" });

            var response = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Requires a valid Bearer token. The current password is validated before the change.
    /// NewPassword and ConfirmPassword must match.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/auth/change-password
    ///     {
    ///         "currentPassword": "OldPass1",
    ///         "newPassword": "NewPass1",
    ///         "confirmPassword": "NewPass1"
    ///     }
    /// </remarks>
    /// <param name="request">Current and new password payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            await _authService.ChangePasswordAsync(userId, request, ct);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
