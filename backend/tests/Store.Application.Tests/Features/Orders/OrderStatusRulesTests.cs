using FluentAssertions;
using Store.Domain.Enums;
using Xunit;

namespace Store.Application.Tests.Features.Orders;

public class OrderStatusRulesTests
{
    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Approved)]
    [InlineData(OrderStatus.New, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Approved, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Approved, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public void CanTransition_ShouldAllowValidMoves(OrderStatus from, OrderStatus to)
        => OrderStatusRules.CanTransition(from, to).Should().BeTrue();

    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Delivered)]
    [InlineData(OrderStatus.New, OrderStatus.New)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.New)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Approved)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
    public void CanTransition_ShouldRejectInvalidMoves(OrderStatus from, OrderStatus to)
        => OrderStatusRules.CanTransition(from, to).Should().BeFalse();

    [Fact]
    public void EveryStatus_ShouldHaveARuleEntry()
    {
        foreach (var status in Enum.GetValues<OrderStatus>())
        {
            var act = () => OrderStatusRules.GetNext(status);
            act.Should().NotThrow($"{status} must be listed in the transition table");
        }
    }
}
