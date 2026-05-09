using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Repositories;

namespace OwnDeliveryApiP33.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;
    private readonly PasswordHasher<User> _passwordHasher;

    public CustomerService(
        IUnitOfWork unitOfWork,
        ILogger<CustomerService> logger,
        PasswordHasher<User> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<CustomerResponse> CreateCustomerAsync(AdminCreateCustomerRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLower();

        if (await _unitOfWork.Users.EmailExistsAsync(email, ct: ct))
            throw new DuplicateEntityException($"User with email '{request.Email}' already exists");

        if (await _unitOfWork.Users.PhoneExistsAsync(request.PhoneNumber, ct: ct))
            throw new DuplicateEntityException($"User with phone number '{request.PhoneNumber}' already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim(),
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.Customer,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            PreferredDeliveryAddress = request.PreferredDeliveryAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, ct);
        await _unitOfWork.Customers.AddAsync(customer, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(customer);
    }

    public async Task<PagedResponse<CustomerResponse>> GetCustomersAsync(int skip = 0, int take = 20, CancellationToken ct = default)
    {
        var normalizedTake = Math.Clamp(take, 1, 100);
        var normalizedSkip = Math.Max(skip, 0);

        var query = _unitOfWork.Customers.GetQueryable()
            .Include(c => c.User)
            .Where(c => c.User.Status != UserStatus.Deleted)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(ct);
        var customers = await query
            .Skip(normalizedSkip)
            .Take(normalizedTake)
            .ToListAsync(ct);

        var items = customers.Select(customer => MapToResponse(customer)).ToList();
        return new PagedResponse<CustomerResponse>(
            items,
            total,
            normalizedSkip,
            normalizedTake,
            normalizedSkip + items.Count < total);
    }

    public async Task<CustomerResponse> GetProfileAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetQueryable()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), customerId);

        var orders = await _unitOfWork.Orders.GetCustomerOrdersAsync(customerId, 0, 1000, ct);
        
        return MapToResponse(customer, orders);
    }

    public async Task<CustomerResponse> UpdateCustomerAsync(Guid customerId, AdminUpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetQueryable()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), customerId);

        if (!string.IsNullOrWhiteSpace(request.FullName))
            customer.User.FullName = request.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != customer.User.PhoneNumber)
        {
            if (await _unitOfWork.Users.PhoneExistsAsync(request.PhoneNumber, customer.UserId, ct))
                throw new DuplicateEntityException($"User with phone number '{request.PhoneNumber}' already exists");

            customer.User.PhoneNumber = request.PhoneNumber;
        }

        if (request.PreferredDeliveryAddress != null)
            customer.PreferredDeliveryAddress = request.PreferredDeliveryAddress;

        if (request.Status.HasValue)
            customer.User.Status = request.Status.Value;

        customer.UpdatedAt = DateTime.UtcNow;
        customer.User.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer, ct);
        await _unitOfWork.Users.UpdateAsync(customer.User, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(customer);
    }

    public async Task<bool> DeleteCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetQueryable()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer == null)
            return false;

        customer.User.Status = UserStatus.Deleted;
        customer.User.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(customer.User, ct);
        await _unitOfWork.Customers.UpdateAsync(customer, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<CustomerResponse> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId, ct);
        if (customer == null)
            throw new EntityNotFoundException("Customer not found for user");

        return await GetProfileAsync(customer.Id, ct);
    }

    public async Task<IEnumerable<CustomerResponse>> GetTopCustomersAsync(int count = 10, CancellationToken ct = default)
    {
        var customers = await _unitOfWork.Customers.GetTopCustomersAsync(count, ct);
        
        var result = new List<CustomerResponse>();
        foreach (var customer in customers)
        {
            result.Add(await GetProfileAsync(customer.Id, ct));
        }

        return result;
    }

    public async Task<CustomerStatsResponse> GetStatsAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, ct);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), customerId);

        var orders = await _unitOfWork.Orders.GetCustomerOrdersAsync(customerId, 0, int.MaxValue, ct);
        var ordersList = orders.ToList();

        var completed = ordersList.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered);
        var cancelled = ordersList.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled);
        var pending = ordersList.Count(o => o.Status != Domain.Enums.OrderStatus.Delivered && o.Status != Domain.Enums.OrderStatus.Cancelled);
        
        var totalSpent = ordersList.Sum(o => o.Cost);
        var avgCost = ordersList.Any() ? totalSpent / ordersList.Count : 0;

        return new CustomerStatsResponse(
            ordersList.Count,
            completed,
            cancelled,
            pending,
            totalSpent,
            avgCost,
            customer.AverageRating
        );
    }

    private static CustomerResponse MapToResponse(Customer customer, IEnumerable<Order>? orders = null)
    {
        var orderList = orders?.ToList();

        return new CustomerResponse(
            customer.Id,
            customer.User?.Email ?? "",
            customer.User?.FullName ?? "",
            customer.User?.PhoneNumber ?? "",
            customer.AverageRating,
            orderList?.Count ?? customer.TotalOrders,
            orderList?.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered) ?? customer.SuccessfulOrders,
            orderList?.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled) ?? customer.CancelledOrders,
            customer.PreferredDeliveryAddress);
    }
}
