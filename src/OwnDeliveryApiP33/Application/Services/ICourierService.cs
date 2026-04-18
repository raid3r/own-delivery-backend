using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Services;

public interface ICourierService : IApplicationService
{
    /// <summary>Get the profile of the currently authenticated courier</summary>
    Task<CourierProfileResponse> GetProfileAsync(Guid courierId, CancellationToken ct = default);
}
