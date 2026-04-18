using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Application.DTOs;
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
            .FirstOrDefaultAsync(c => c.Id == courierId, ct);

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
}
