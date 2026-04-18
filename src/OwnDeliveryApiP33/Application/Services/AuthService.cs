using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Validators;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterCourierRequest> _registerValidator;
    private readonly IValidator<LoginCourierRequest> _loginValidator;
    private readonly IValidator<LoginRequest> _loginRequestValidator;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        ApplicationDbContext context,
        ITokenService tokenService,
        IValidator<RegisterCourierRequest> registerValidator,
        IValidator<LoginCourierRequest> loginValidator,
        IValidator<LoginRequest> loginRequestValidator,
        PasswordHasher<User> passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _loginRequestValidator = loginRequestValidator;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterCourierRequest request, CancellationToken ct = default)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = $"{request.FirstName} {request.LastName}",
            Email = request.Email.ToLower(),
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.Courier,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var courier = new Courier
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            LicenseNumber = request.LicenseNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Couriers.Add(courier);
        await _context.SaveChangesAsync(ct);

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        return new AuthResponse(
            user.Id,
            user.Email,
            request.FirstName,
            request.LastName,
            token,
            expiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginCourierRequest request, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        var fullName = user.FullName.Split(' ');
        return new AuthResponse(
            user.Id,
            user.Email,
            fullName[0],
            fullName.Length > 1 ? fullName[1] : "",
            token,
            expiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = await _loginRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        var fullName = user.FullName.Split(' ');
        return new AuthResponse(
            user.Id,
            user.Email,
            fullName[0],
            fullName.Length > 1 ? fullName[1] : "",
            token,
            expiresAt);
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return false;

        // In a real implementation, we would invalidate the token here
        // For now, we just return true as JWT tokens are stateless
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        // TODO: Implement email verification token logic
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
        if (user == null)
            return false;

        // TODO: Generate password reset token and send email
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        // TODO: Validate token and update password
        await Task.CompletedTask;
        return true;
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is required.");
        }

        // In a real implementation, we would verify the refresh token against stored tokens in database
        // For now, we'll just generate a new token
        // TODO: Store and validate refresh tokens in database

        var (newAccessToken, expiresAt) = _tokenService.GenerateToken(new User
        {
            Id = Guid.NewGuid(),
            Email = "placeholder@example.com",
            FullName = "Placeholder",
            PhoneNumber = "placeholder"
        });

        var (newRefreshToken, refreshExpiresAt) = _tokenService.GenerateRefreshToken();

        return new RefreshTokenResponse(newAccessToken, newRefreshToken, expiresAt);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return false;

        // Verify current password
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Current password is incorrect");

        // Hash and set new password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
        
        return true;
    }
}
