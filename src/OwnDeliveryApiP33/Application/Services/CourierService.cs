using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.ValueObjects;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Application.Services;

public class CourierService : ICourierService
{
    private readonly ApplicationDbContext _context;

    public CourierService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourierProfileResponse> GetProfileAsync(Guid courierId, CancellationToken ct = default)
    {
        var courier = await _context.Couriers
            .Include(c => c.User)
            // JWT `sub` currently stores User.Id; support direct courier lookup as well.
            .FirstOrDefaultAsync(c => c.Id == courierId || c.UserId == courierId, ct);

        if (courier is null || courier.User is null)
        {
            throw new KeyNotFoundException("Courier not found.");
        }

        var user = courier.User;
        var nameParts = user.FullName.Split(' ');

        return new CourierProfileResponse(
            courier.Id,
            user.Email,
            nameParts[0],
            nameParts.Length > 1 ? nameParts[1] : "",
            user.PhoneNumber,
            courier.CreatedAt,
            user.Email != null);
    }

    public async Task UpdateLocationAsync(Guid courierUserId, LocationDto location, CancellationToken ct = default)
    {
        ValidateLocation(location);

        var courier = await _context.Couriers
            // JWT `sub` currently stores User.Id; support direct courier lookup as well.
            .FirstOrDefaultAsync(c => c.Id == courierUserId || c.UserId == courierUserId, ct);

        if (courier is null)
        {
            throw new KeyNotFoundException("Courier not found.");
        }

        var now = DateTime.UtcNow;

        _context.CourierLocations.Add(new CourierLocation
        {
            CourierId = courier.Id,
            Location = new Location(
                location.Latitude,
                location.Longitude,
                location.Accuracy,
                location.Altitude,
                location.Speed),
            Timestamp = now,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _context.SaveChangesAsync(ct);
    }

    private static void ValidateLocation(LocationDto location)
    {
        if (location.Latitude is < -90 or > 90)
        {
            throw new ArgumentException("Latitude must be between -90 and 90.");
        }

        if (location.Longitude is < -180 or > 180)
        {
            throw new ArgumentException("Longitude must be between -180 and 180.");
        }

        if (location.Accuracy is <= 0)
        {
            throw new ArgumentException("Accuracy must be greater than 0.");
        }

        if (location.Speed is < 0)
        {
            throw new ArgumentException("Speed must be greater than or equal to 0.");
        }
    }
}
