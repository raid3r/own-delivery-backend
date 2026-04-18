using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class TariffRepository : Repository<Tariff>, ITariffRepository
{
    public TariffRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Tariff>> GetActiveTariffsAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<Tariff?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name == name, ct);
    }

    public async Task<Tariff?> GetDefaultTariffAsync(CancellationToken ct = default)
    {
        // Get the first active tariff or one with the lowest base cost
        return await _dbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.BaseCost)
            .FirstOrDefaultAsync(ct);
    }
}
