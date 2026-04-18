using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetCompletedPaymentsAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
