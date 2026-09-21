using FluentAssertions;
using Store.Application.Common.Exceptions;
using Store.Application.Features.Orders.Commands.UpdateOrderStatus;
using Store.Application.Tests.Common;
using Store.Domain.Entities;
using Store.Domain.Enums;
using Store.Infrastructure.Persistence;
using Xunit;

namespace Store.Application.Tests.Features.Orders;

public class UpdateOrderStatusCommandHandlerTests
{
    // Stock is 10 - 3 = 7 after an order for 3 was placed (stock is reserved at order time).
    private static async Task<(StoreDbContext context, Order order, Product product)> ArrangeOrderAsync(
        OrderStatus status = OrderStatus.New, bool productActive = true)
    {
        var context = TestDbContextFactory.Create();
        var user = new User { Email = "buyer@example.com", FullName = "Buyer", PasswordHash = "hash" };
        var category = new Category { Name = "Electronics", Slug = "electronics" };
        var product = new Product
        {
            Name = "Mouse", Slug = "mouse", Price = 10m, StockQuantity = 7,
            CategoryId = category.Id, Category = category, IsActive = productActive
        };
        var order = new Order
        {
            UserId = user.Id, User = user, Status = status, ShippingAddress = "1 Main St",
            ContactPhone = "+7 900 000-00-00", TotalAmount = 30m
        };
        order.Items.Add(new OrderItem { OrderId = order.Id, ProductId = product.Id, Product = product, ProductName = "Mouse", Price = 10m, Quantity = 3 });

        context.Users.Add(user);
        context.Categories.Add(category);
        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return (context, order, product);
    }

    private static UpdateOrderStatusCommandHandler HandlerFor(StoreDbContext context)
        => new(context, MapperFactory.Create());

    [Fact]
    public async Task Approve_ShouldKeepStockUnchanged()
    {
        var (context, order, product) = await ArrangeOrderAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(
            new UpdateOrderStatusCommand { Id = order.Id, Status = OrderStatus.Approved }, CancellationToken.None);

        result.Status.Should().Be("Approved");
        (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(7);
    }

    [Fact]
    public async Task Reject_ShouldReturnReservedStock()
    {
        var (context, order, product) = await ArrangeOrderAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(
            new UpdateOrderStatusCommand { Id = order.Id, Status = OrderStatus.Cancelled }, CancellationToken.None);

        result.Status.Should().Be("Cancelled");
        (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Reject_ShouldReturnStock_EvenIfProductWasDeactivatedMeanwhile()
    {
        var (context, order, product) = await ArrangeOrderAsync(productActive: false);
        using var _ = context;

        await HandlerFor(context).Handle(
            new UpdateOrderStatusCommand { Id = order.Id, Status = OrderStatus.Cancelled }, CancellationToken.None);

        (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Reject_Twice_ShouldNotReturnStockTwice()
    {
        var (context, order, product) = await ArrangeOrderAsync();
        using var _ = context;
        var handler = HandlerFor(context);
        var command = new UpdateOrderStatusCommand { Id = order.Id, Status = OrderStatus.Cancelled };

        await handler.Handle(command, CancellationToken.None);
        var second = () => handler.Handle(command, CancellationToken.None);

        await second.Should().ThrowAsync<ConflictException>();
        (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(10, "stock must be returned exactly once");
    }

    [Fact]
    public async Task CancelledOrder_CannotBeApprovedAgain()
    {
        var (context, order, product) = await ArrangeOrderAsync(OrderStatus.Cancelled);
        using var _ = context;

        var act = () => HandlerFor(context).Handle(
            new UpdateOrderStatusCommand { Id = order.Id, Status = OrderStatus.Approved }, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(7);
    }

    [Fact]
    public async Task UnknownOrder_ShouldThrowNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var act = () => HandlerFor(context).Handle(
            new UpdateOrderStatusCommand { Id = Guid.NewGuid(), Status = OrderStatus.Approved }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Result_ShouldExposeAllowedNextStatuses()
    {
        var (context, order, _) = await ArrangeOrderAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(
            new UpdateOrderStatusCommand { Id = order.Id, Status = OrderStatus.Approved }, CancellationToken.None);

        result.AllowedNextStatuses.Should().BeEquivalentTo("Processing", "Shipped", "Cancelled");
    }
}
