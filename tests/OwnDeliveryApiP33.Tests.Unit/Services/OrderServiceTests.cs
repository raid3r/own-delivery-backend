using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OwnDeliveryApiP33.Application.Exceptions;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Domain.ValueObjects;
using OwnDeliveryApiP33.Infrastructure.Repositories;

namespace OwnDeliveryApiP33.Tests.Unit.Services;

public class OrderServiceTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly ICourierRepository _courierRepo = Substitute.For<ICourierRepository>();
    private readonly ILogger<OrderService> _logger = Substitute.For<ILogger<OrderService>>();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _unitOfWork.Orders.Returns(_orderRepo);
        _unitOfWork.Couriers.Returns(_courierRepo);
        _sut = new OrderService(_unitOfWork, _logger);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Order BuildOrder(
        OrderStatus status = OrderStatus.Pending,
        Guid? courierId = null) => new()
    {
        Id = Guid.NewGuid(),
        OrderNumber = "ORD-TEST-001",
        CustomerId = Guid.NewGuid(),
        Status = status,
        CourierId = courierId,
        PickupAddress = new Address("Kyiv", "Main St", "1", "01001", 50.4m, 30.5m),
        DeliveryAddress = new Address("Kyiv", "Side St", "2", "01002", 50.5m, 30.6m),
        Weight = 1m,
        Dimensions = new Dimensions(10, 10, 10),
        Cost = 100m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    private static Courier BuildCourier(Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // ── AcceptOrderAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task AcceptOrderAsync_WithValidOrder_ReturnsOrderResponse()
    {
        var order = BuildOrder(OrderStatus.Pending);
        var courier = BuildCourier();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(courier.UserId).Returns(courier);

        var result = await _sut.AcceptOrderAsync(order.Id, courier.UserId);

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
        result.Status.Should().Be(OrderStatus.Assigned);
    }

    [Fact]
    public async Task AcceptOrderAsync_SetsStatusToAccepted()
    {
        var order = BuildOrder(OrderStatus.Pending);
        var courier = BuildCourier();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(courier.UserId).Returns(courier);

        await _sut.AcceptOrderAsync(order.Id, courier.UserId);

        order.Status.Should().Be(OrderStatus.Assigned);
        order.CourierId.Should().Be(courier.Id);
    }

    [Fact]
    public async Task AcceptOrderAsync_SavesChanges()
    {
        var order = BuildOrder(OrderStatus.Pending);
        var courier = BuildCourier();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(courier.UserId).Returns(courier);

        await _sut.AcceptOrderAsync(order.Id, courier.UserId);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AcceptOrderAsync_WhenOrderNotFound_ThrowsEntityNotFoundException()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Order?)null);

        var act = () => _sut.AcceptOrderAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AcceptOrderAsync_WhenAlreadyAssigned_ThrowsInvalidOperationException()
    {
        var existingCourierId = Guid.NewGuid();
        var order = BuildOrder(OrderStatus.Assigned, existingCourierId);
        var courier = BuildCourier();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(courier.UserId).Returns(courier);

        var act = () => _sut.AcceptOrderAsync(order.Id, courier.UserId);

        await act.Should().ThrowAsync<OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException>()
            .WithMessage($"*{order.Id}*");
    }

    [Theory]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task AcceptOrderAsync_WithNonAcceptableStatus_ThrowsInvalidOperationException(OrderStatus status)
    {
        var order = BuildOrder(status);
        var courier = BuildCourier();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(courier.UserId).Returns(courier);

        var act = () => _sut.AcceptOrderAsync(order.Id, courier.UserId);

        await act.Should().ThrowAsync<OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException>();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.PickedUp)]
    public async Task AcceptOrderAsync_WithAcceptableStatus_Succeeds(OrderStatus status)
    {
        var order = BuildOrder(status);
        var courier = BuildCourier();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(courier.UserId).Returns(courier);

        var act = () => _sut.AcceptOrderAsync(order.Id, courier.UserId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AcceptOrderAsync_WhenCourierProfileNotFound_ThrowsEntityNotFoundException()
    {
        var order = BuildOrder(OrderStatus.Pending);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(Arg.Any<Guid>()).Returns((Courier?)null);

        var act = () => _sut.AcceptOrderAsync(order.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── GetAvailableOrdersAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetCourierOrdersAsync_ReturnsPagedResponse()
    {
        var courierId = Guid.NewGuid();
        var orders = new List<Order> { BuildOrder(courierId: courierId), BuildOrder(courierId: courierId) };
        _orderRepo.CountAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Order, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(2);
        _orderRepo.GetCourierOrdersAsync(courierId, 0, 20, Arg.Any<CancellationToken>()).Returns(orders);

        var result = await _sut.GetCourierOrdersAsync(courierId);

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_ReturnsPagedResponse()
    {
        var orders = new List<Order> { BuildOrder(), BuildOrder() };
        _orderRepo.CountUnassignedOrdersAsync(null, null, null, Arg.Any<CancellationToken>()).Returns(2);
        _orderRepo.GetUnassignedOrdersAsync(0, 20, null, null, null, Arg.Any<CancellationToken>()).Returns(orders);

        var result = await _sut.GetAvailableOrdersAsync();

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_HasMoreTrue_WhenMorePagesExist()
    {
        var orders = Enumerable.Range(0, 5).Select(_ => BuildOrder()).ToList();
        _orderRepo.CountUnassignedOrdersAsync(null, null, null, Arg.Any<CancellationToken>()).Returns(10);
        _orderRepo.GetUnassignedOrdersAsync(0, 5, null, null, null, Arg.Any<CancellationToken>()).Returns(orders);

        var result = await _sut.GetAvailableOrdersAsync(skip: 0, take: 5);

        result.HasMore.Should().BeTrue();
        result.Total.Should().Be(10);
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_WithGeoFilter_PassesCoordinatesToRepository()
    {
        _orderRepo.CountUnassignedOrdersAsync(50.4501m, 30.5234m, 5m, Arg.Any<CancellationToken>()).Returns(0);
        _orderRepo.GetUnassignedOrdersAsync(0, 20, 50.4501m, 30.5234m, 5m, Arg.Any<CancellationToken>())
            .Returns(new List<Order>());

        await _sut.GetAvailableOrdersAsync(lat: 50.4501m, lon: 30.5234m, radiusKm: 5m);

        await _orderRepo.Received(1).CountUnassignedOrdersAsync(50.4501m, 30.5234m, 5m, Arg.Any<CancellationToken>());
        await _orderRepo.Received(1).GetUnassignedOrdersAsync(0, 20, 50.4501m, 30.5234m, 5m, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_ClampsTakeTo100()
    {
        _orderRepo.CountUnassignedOrdersAsync(null, null, null, Arg.Any<CancellationToken>()).Returns(0);
        _orderRepo.GetUnassignedOrdersAsync(0, 100, null, null, null, Arg.Any<CancellationToken>()).Returns(new List<Order>());

        await _sut.GetAvailableOrdersAsync(take: 999);

        await _orderRepo.Received(1).GetUnassignedOrdersAsync(0, 100, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_NormalizesNegativeSkipToZero()
    {
        _orderRepo.CountUnassignedOrdersAsync(null, null, null, Arg.Any<CancellationToken>()).Returns(0);
        _orderRepo.GetUnassignedOrdersAsync(0, 20, null, null, null, Arg.Any<CancellationToken>()).Returns(new List<Order>());

        await _sut.GetAvailableOrdersAsync(skip: -5);

        await _orderRepo.Received(1).GetUnassignedOrdersAsync(0, 20, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AcceptOrderAsync_WhenConcurrentUpdateOccurs_ThrowsInvalidOperationException()
    {
        var order = BuildOrder(OrderStatus.Pending);
        var courier = BuildCourier();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _courierRepo.GetByUserIdAsync(courier.UserId).Returns(courier);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new DbUpdateConcurrencyException());

        var act = () => _sut.AcceptOrderAsync(order.Id, courier.UserId);

        await act.Should().ThrowAsync<OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException>()
            .WithMessage("*already been assigned*");
    }
}
