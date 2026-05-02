using FluentAssertions;
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
    private readonly ILogger<OrderService> _logger = Substitute.For<ILogger<OrderService>>();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _unitOfWork.Orders.Returns(_orderRepo);
        _sut = new OrderService(_unitOfWork, _logger);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Order BuildOrder(
        OrderStatus status = OrderStatus.New,
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

    // ── AcceptOrderAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task AcceptOrderAsync_WithValidOrder_ReturnsOrderResponse()
    {
        var order = BuildOrder(OrderStatus.New);
        var courierId = Guid.NewGuid();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);

        var result = await _sut.AcceptOrderAsync(order.Id, courierId);

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
        result.Status.Should().Be(OrderStatus.Accepted);
    }

    [Fact]
    public async Task AcceptOrderAsync_SetsStatusToAccepted()
    {
        var order = BuildOrder(OrderStatus.New);
        var courierId = Guid.NewGuid();
        _orderRepo.GetByIdAsync(order.Id).Returns(order);

        await _sut.AcceptOrderAsync(order.Id, courierId);

        order.Status.Should().Be(OrderStatus.Accepted);
        order.CourierId.Should().Be(courierId);
    }

    [Fact]
    public async Task AcceptOrderAsync_SavesChanges()
    {
        var order = BuildOrder(OrderStatus.New);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);

        await _sut.AcceptOrderAsync(order.Id, Guid.NewGuid());

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
        var order = BuildOrder(OrderStatus.Accepted, existingCourierId);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);

        var act = () => _sut.AcceptOrderAsync(order.Id, Guid.NewGuid());

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
        _orderRepo.GetByIdAsync(order.Id).Returns(order);

        var act = () => _sut.AcceptOrderAsync(order.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException>();
    }

    [Theory]
    [InlineData(OrderStatus.New)]
    [InlineData(OrderStatus.WaitingForCourier)]
    public async Task AcceptOrderAsync_WithAcceptableStatus_Succeeds(OrderStatus status)
    {
        var order = BuildOrder(status);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);

        var act = () => _sut.AcceptOrderAsync(order.Id, Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    // ── GetAvailableOrdersAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableOrdersAsync_ReturnsPagedResponse()
    {
        var orders = new List<Order> { BuildOrder(), BuildOrder() };
        _orderRepo.CountUnassignedOrdersAsync().Returns(2);
        _orderRepo.GetUnassignedOrdersAsync(0, 20).Returns(orders);

        var result = await _sut.GetAvailableOrdersAsync();

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_HasMoreTrue_WhenMorePagesExist()
    {
        var orders = Enumerable.Range(0, 5).Select(_ => BuildOrder()).ToList();
        _orderRepo.CountUnassignedOrdersAsync().Returns(10);
        _orderRepo.GetUnassignedOrdersAsync(0, 5).Returns(orders);

        var result = await _sut.GetAvailableOrdersAsync(skip: 0, take: 5);

        result.HasMore.Should().BeTrue();
        result.Total.Should().Be(10);
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_ClampsTakeTo100()
    {
        _orderRepo.CountUnassignedOrdersAsync().Returns(0);
        _orderRepo.GetUnassignedOrdersAsync(0, 100).Returns(new List<Order>());

        await _sut.GetAvailableOrdersAsync(take: 999);

        await _orderRepo.Received(1).GetUnassignedOrdersAsync(0, 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAvailableOrdersAsync_NormalizesNegativeSkipToZero()
    {
        _orderRepo.CountUnassignedOrdersAsync().Returns(0);
        _orderRepo.GetUnassignedOrdersAsync(0, 20).Returns(new List<Order>());

        await _sut.GetAvailableOrdersAsync(skip: -5);

        await _orderRepo.Received(1).GetUnassignedOrdersAsync(0, 20, Arg.Any<CancellationToken>());
    }
}
