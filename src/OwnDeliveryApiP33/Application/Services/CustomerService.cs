using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Infrastructure.Repositories;

namespace OwnDeliveryApiP33.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CustomerResponse> GetProfileAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, ct);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), customerId);

        var orders = await _unitOfWork.Orders.GetCustomerOrdersAsync(customerId, 0, 1000, ct);
        
        return new CustomerResponse(
            customer.Id,
            customer.User?.Email ?? "",
            customer.User?.FullName ?? "",
            customer.User?.PhoneNumber ?? "",
            customer.AverageRating,
            orders?.Count() ?? 0,
            orders?.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered) ?? 0,
            orders?.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled) ?? 0
        );
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
}
