using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<IEnumerable<Customer>> GetTopCustomersAsync(int count = 10, CancellationToken ct = default)
    {
        return await _dbSet
            .OrderByDescending(c => c.TotalOrders)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalSpentAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await GetByIdAsync(customerId, ct);
        if (customer == null)
            return 0;

        // Calculate total from orders
        var totalSpent = customer.Orders?.Sum(o => o.Cost) ?? 0;
        return totalSpent;
    }
}
