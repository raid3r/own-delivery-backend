using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Domain.ValueObjects;
using OwnDeliveryApiP33.Infrastructure.Repositories;

namespace OwnDeliveryApiP33.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(Guid customerId, CreateOrderRequest request, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, ct);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Customer), customerId);

        var tariff = await _unitOfWork.Tariffs.GetByIdAsync(request.TariffId, ct);
        if (tariff == null)
            throw new EntityNotFoundException(nameof(Tariff), request.TariffId);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            TariffId = request.TariffId,
            Status = OrderStatus.New,
            PaymentStatus = PaymentStatus.Pending,
            PickupAddress = new Address(
                request.PickupAddress.City,
                request.PickupAddress.Street,
                request.PickupAddress.BuildingNumber,
                request.PickupAddress.PostalCode,
                request.PickupAddress.Latitude,
                request.PickupAddress.Longitude,
                request.PickupAddress.ApartmentNumber,
                request.PickupAddress.Description
            ),
            DeliveryAddress = new Address(
                request.DeliveryAddress.City,
                request.DeliveryAddress.Street,
                request.DeliveryAddress.BuildingNumber,
                request.DeliveryAddress.PostalCode,
                request.DeliveryAddress.Latitude,
                request.DeliveryAddress.Longitude,
                request.DeliveryAddress.ApartmentNumber,
                request.DeliveryAddress.Description
            ),
            Weight = request.Weight,
            Dimensions = new Dimensions(request.Dimensions.Width, request.Dimensions.Length, request.Dimensions.Height),
            Cost = CalculateOrderCost(tariff, request),
            Description = request.Description,
            SpecialInstructions = request.SpecialInstructions,
            EstimatedDeliveryTime = request.ScheduledDeliveryTime ?? DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Orders.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse> GetOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct);
        if (order == null)
            throw new EntityNotFoundException(nameof(Order), orderId);

        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber, ct);
        if (order == null)
            throw new EntityNotFoundException($"Order {orderNumber} not found");

        return MapToOrderResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetCustomerOrdersAsync(Guid customerId, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        var orders = await _unitOfWork.Orders.GetCustomerOrdersAsync(customerId, skip, take, ct);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetCourierOrdersAsync(Guid courierId, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        var orders = await _unitOfWork.Orders.GetCourierOrdersAsync(courierId, skip, take, ct);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, OrderStatusUpdateRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct);
        if (order == null)
            throw new EntityNotFoundException(nameof(Order), orderId);

        var oldStatus = order.Status;
        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (request.NewStatus == OrderStatus.Delivered)
        {
            order.ActualDeliveryTime = DateTime.UtcNow;
            order.PaymentStatus = PaymentStatus.Completed;
        }

        await _unitOfWork.Orders.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToOrderResponse(order);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct);
        if (order == null)
            return false;

        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
            throw new OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException($"Cannot cancel order in {order.Status} status");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RateOrderAsync(Guid orderId, RateOrderRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct);
        if (order == null)
            return false;

        if (order.Status != OrderStatus.Delivered)
            throw new OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException("Can only rate delivered orders");

        var courierId = order.CourierId;
        if (courierId == null)
            throw new OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException("Order has no assigned courier");

        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CourierId = courierId.Value,
            CustomerId = order.CustomerId,
            Score = request.Score,
            Comment = request.Comment,
            Type = RatingType.Delivery,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Ratings.AddAsync(rating, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            new AddressDto(
                order.PickupAddress.City,
                order.PickupAddress.Street,
                order.PickupAddress.BuildingNumber,
                order.PickupAddress.PostalCode,
                order.PickupAddress.Latitude,
                order.PickupAddress.Longitude,
                order.PickupAddress.ApartmentNumber,
                order.PickupAddress.Description
            ),
            new AddressDto(
                order.DeliveryAddress.City,
                order.DeliveryAddress.Street,
                order.DeliveryAddress.BuildingNumber,
                order.DeliveryAddress.PostalCode,
                order.DeliveryAddress.Latitude,
                order.DeliveryAddress.Longitude,
                order.DeliveryAddress.ApartmentNumber,
                order.DeliveryAddress.Description
            ),
            order.Weight,
            new DimensionsDto(order.Dimensions.Width, order.Dimensions.Length, order.Dimensions.Height),
            order.Cost,
            order.PaymentStatus,
            order.CreatedAt,
            order.ActualDeliveryTime,
            order.Description,
            order.SpecialInstructions
        );
    }

    private decimal CalculateOrderCost(Tariff tariff, CreateOrderRequest request)
    {
        // Simple calculation: base cost + weight cost + dimensions cost
        var baseCost = tariff.BaseCost;
        var weightCost = request.Weight * tariff.PricePerKg;
        // For simplicity, not calculating distance - would need geo coordinates distance
        
        return baseCost + weightCost;
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}
