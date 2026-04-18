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

/// <summary>
/// Request payload for creating a tariff.
/// </summary>
/// <param name="Name">Tariff name.</param>
/// <param name="BaseCost">Base order cost.</param>
/// <param name="PricePerKm">Price per kilometer.</param>
/// <param name="PricePerKg">Price per kilogram.</param>
/// <param name="EstimatedDeliveryTime">Estimated delivery time in minutes.</param>
/// <param name="MaxWeight">Maximum package weight in kilograms.</param>
/// <param name="MaxDimensions">Maximum package dimensions.</param>
/// <param name="Description">Optional tariff description.</param>
public record CreateTariffRequest(
    string Name,
    decimal BaseCost,
    decimal PricePerKm,
    decimal PricePerKg,
    int EstimatedDeliveryTime,
    decimal MaxWeight,
    DimensionsDto MaxDimensions,
    string? Description = null);

/// <summary>
/// Request payload for updating an existing tariff.
/// </summary>
/// <param name="Name">Updated tariff name.</param>
/// <param name="BaseCost">Updated base order cost.</param>
/// <param name="PricePerKm">Updated price per kilometer.</param>
/// <param name="PricePerKg">Updated price per kilogram.</param>
/// <param name="EstimatedDeliveryTime">Updated estimated delivery time in minutes.</param>
/// <param name="MaxWeight">Updated maximum package weight in kilograms.</param>
/// <param name="MaxDimensions">Updated maximum package dimensions.</param>
/// <param name="Description">Updated tariff description.</param>
/// <param name="IsActive">Updated active status flag.</param>
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
