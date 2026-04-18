using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default);
    Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludeUserId = null, CancellationToken ct = default);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default);
    Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken ct = default);
}
