using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface ITariffRepository : IRepository<Tariff>
{
    Task<IEnumerable<Tariff>> GetActiveTariffsAsync(CancellationToken ct = default);
    Task<Tariff?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Tariff?> GetDefaultTariffAsync(CancellationToken ct = default);
}
