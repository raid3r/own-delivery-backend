using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(u => u.Email == email.ToLower());
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        
        return await query.AnyAsync(ct);
    }

    public async Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(u => u.PhoneNumber == phoneNumber);
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        
        return await query.AnyAsync(ct);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync(ct);
    }

    public async Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken ct = default)
    {
        return await _dbSet.Where(u => u.Status == status).ToListAsync(ct);
    }
}
