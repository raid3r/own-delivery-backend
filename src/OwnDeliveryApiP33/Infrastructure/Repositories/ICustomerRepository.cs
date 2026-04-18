using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Customer>> GetTopCustomersAsync(int count = 10, CancellationToken ct = default);
    Task<decimal> GetTotalSpentAsync(Guid customerId, CancellationToken ct = default);
}
