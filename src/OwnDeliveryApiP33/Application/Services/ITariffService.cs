using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Services;

public interface ITariffService : IApplicationService
{
    /// <summary>Get tariff by ID</summary>
    Task<TariffResponse> GetTariffAsync(Guid tariffId, CancellationToken ct = default);

    /// <summary>Get all active tariffs</summary>
    Task<IEnumerable<TariffResponse>> GetActiveTariffsAsync(CancellationToken ct = default);

    /// <summary>Get tariff by name</summary>
    Task<TariffResponse> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Get default tariff</summary>
    Task<TariffResponse> GetDefaultTariffAsync(CancellationToken ct = default);

    /// <summary>Create new tariff (admin only)</summary>
    Task<TariffResponse> CreateTariffAsync(CreateTariffRequest request, CancellationToken ct = default);

    /// <summary>Update tariff (admin only)</summary>
    Task<TariffResponse> UpdateTariffAsync(Guid tariffId, UpdateTariffRequest request, CancellationToken ct = default);

    /// <summary>Deactivate tariff (admin only)</summary>
    Task<bool> DeactivateTariffAsync(Guid tariffId, CancellationToken ct = default);
}

public record CreateTariffRequest(
    string Name,
    decimal BaseCost,
    decimal PricePerKm,
    decimal PricePerKg,
    int EstimatedDeliveryTime,
    decimal MaxWeight,
    DimensionsDto MaxDimensions,
    string? Description = null);

public record UpdateTariffRequest(
    string? Name = null,
    decimal? BaseCost = null,
    decimal? PricePerKm = null,
    decimal? PricePerKg = null,
    int? EstimatedDeliveryTime = null,
    decimal? MaxWeight = null,
    DimensionsDto? MaxDimensions = null,
    string? Description = null,
    bool? IsActive = null);
