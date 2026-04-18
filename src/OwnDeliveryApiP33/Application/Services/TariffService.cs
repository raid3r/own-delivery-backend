using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.ValueObjects;
using OwnDeliveryApiP33.Infrastructure.Repositories;

namespace OwnDeliveryApiP33.Application.Services;

public class TariffService : ITariffService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TariffService> _logger;

    public TariffService(IUnitOfWork unitOfWork, ILogger<TariffService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TariffResponse> GetTariffAsync(Guid tariffId, CancellationToken ct = default)
    {
        var tariff = await _unitOfWork.Tariffs.GetByIdAsync(tariffId, ct);
        if (tariff == null)
            throw new EntityNotFoundException(nameof(Tariff), tariffId);

        return MapToResponse(tariff);
    }

    public async Task<IEnumerable<TariffResponse>> GetActiveTariffsAsync(CancellationToken ct = default)
    {
        var tariffs = await _unitOfWork.Tariffs.GetActiveTariffsAsync(ct);
        return tariffs.Select(MapToResponse);
    }

    public async Task<TariffResponse> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var tariff = await _unitOfWork.Tariffs.GetByNameAsync(name, ct);
        if (tariff == null)
            throw new EntityNotFoundException($"Tariff '{name}' not found");

        return MapToResponse(tariff);
    }

    public async Task<TariffResponse> GetDefaultTariffAsync(CancellationToken ct = default)
    {
        var tariff = await _unitOfWork.Tariffs.GetDefaultTariffAsync(ct);
        if (tariff == null)
            throw new EntityNotFoundException("No default tariff found");

        return MapToResponse(tariff);
    }

    public async Task<TariffResponse> CreateTariffAsync(CreateTariffRequest request, CancellationToken ct = default)
    {
        var exists = await _unitOfWork.Tariffs.GetByNameAsync(request.Name, ct);
        if (exists != null)
            throw new DuplicateEntityException($"Tariff with name '{request.Name}' already exists");

        var tariff = new Tariff
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BaseCost = request.BaseCost,
            PricePerKm = request.PricePerKm,
            PricePerKg = request.PricePerKg,
            EstimatedDeliveryTime = request.EstimatedDeliveryTime,
            MaxWeight = request.MaxWeight,
            MaxDimensions = new Dimensions(request.MaxDimensions.Width, request.MaxDimensions.Length, request.MaxDimensions.Height),
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tariffs.AddAsync(tariff, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(tariff);
    }

    public async Task<TariffResponse> UpdateTariffAsync(Guid tariffId, UpdateTariffRequest request, CancellationToken ct = default)
    {
        var tariff = await _unitOfWork.Tariffs.GetByIdAsync(tariffId, ct);
        if (tariff == null)
            throw new EntityNotFoundException(nameof(Tariff), tariffId);

        if (!string.IsNullOrEmpty(request.Name) && request.Name != tariff.Name)
        {
            var exists = await _unitOfWork.Tariffs.GetByNameAsync(request.Name, ct);
            if (exists != null)
                throw new DuplicateEntityException($"Tariff with name '{request.Name}' already exists");
            
            tariff.Name = request.Name;
        }

        if (request.BaseCost.HasValue)
            tariff.BaseCost = request.BaseCost.Value;

        if (request.PricePerKm.HasValue)
            tariff.PricePerKm = request.PricePerKm.Value;

        if (request.PricePerKg.HasValue)
            tariff.PricePerKg = request.PricePerKg.Value;

        if (request.EstimatedDeliveryTime.HasValue)
            tariff.EstimatedDeliveryTime = request.EstimatedDeliveryTime.Value;

        if (request.MaxWeight.HasValue)
            tariff.MaxWeight = request.MaxWeight.Value;

        if (request.MaxDimensions != null)
            tariff.MaxDimensions = new Dimensions(request.MaxDimensions.Width, request.MaxDimensions.Length, request.MaxDimensions.Height);

        if (!string.IsNullOrEmpty(request.Description))
            tariff.Description = request.Description;

        if (request.IsActive.HasValue)
            tariff.IsActive = request.IsActive.Value;

        tariff.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tariffs.UpdateAsync(tariff, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(tariff);
    }

    public async Task<bool> DeactivateTariffAsync(Guid tariffId, CancellationToken ct = default)
    {
        var tariff = await _unitOfWork.Tariffs.GetByIdAsync(tariffId, ct);
        if (tariff == null)
            return false;

        tariff.IsActive = false;
        tariff.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tariffs.UpdateAsync(tariff, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private TariffResponse MapToResponse(Tariff tariff)
    {
        return new TariffResponse(
            tariff.Id,
            tariff.Name,
            tariff.BaseCost,
            tariff.PricePerKm,
            tariff.PricePerKg,
            tariff.EstimatedDeliveryTime,
            tariff.MaxWeight,
            new DimensionsDto(tariff.MaxDimensions.Width, tariff.MaxDimensions.Length, tariff.MaxDimensions.Height),
            tariff.IsActive,
            tariff.Description
        );
    }
}
