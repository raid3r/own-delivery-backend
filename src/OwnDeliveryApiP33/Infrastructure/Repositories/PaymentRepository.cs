using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Payment>> GetCompletedPaymentsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p => p.Status == PaymentStatus.Completed && p.CreatedAt >= from && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }
}
