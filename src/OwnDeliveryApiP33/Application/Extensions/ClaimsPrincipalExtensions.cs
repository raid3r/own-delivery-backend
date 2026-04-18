using System.Security.Claims;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Get user ID from claims</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("sub") ?? user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim?.Value == null)
            throw new UnauthorizedAccessException("User ID not found in claims");
        
        return Guid.Parse(claim.Value);
    }

    /// <summary>Get user role from claims</summary>
    public static UserRole GetUserRole(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.Role) ?? user.FindFirst("role");
        if (claim?.Value == null)
            throw new UnauthorizedAccessException("User role not found in claims");
        
        return Enum.Parse<UserRole>(claim.Value);
    }

    /// <summary>Get user email from claims</summary>
    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.Email);
        if (claim?.Value == null)
            throw new UnauthorizedAccessException("User email not found in claims");
        
        return claim.Value;
    }

    /// <summary>Check if user has specific role</summary>
    public static bool HasRole(this ClaimsPrincipal user, UserRole role)
    {
        try
        {
            var userRole = user.GetUserRole();
            return userRole == role;
        }
        catch
        {
            return false;
        }
    }
}
