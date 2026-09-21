using FluentAssertions;
using Moq;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Orders.Commands.CreateOrder;
using Store.Application.Tests.Common;
using Store.Domain.Entities;
using Xunit;

namespace Store.Application.Tests.Features.Orders;

public class CreateOrderCommandHandlerTests
{
    private static (User user, Category category) SeedUserAndCategory(Infrastructure.Persistence.StoreDbContext context)
    {
        var user = new User { Email = "buyer@example.com", FullName = "Buyer", PasswordHash = "hash" };
        var category = new Category { Name = "Electronics", Slug = "electronics" };
        context.Users.Add(user);
        context.Categories.Add(category);
        context.SaveChanges();
        return (user, category);
    }

    private static ICurrentUserService CurrentUserFor(Guid userId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(x => x.UserId).Returns(userId);
        return mock.Object;
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalAndDecrementStock_WhenStockIsSufficient()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var (user, category) = SeedUserAndCategory(context);

        var cable = new Product { Name = "USB Cable", Slug = "usb-cable", Price = 5.99m, StockQuantity = 100, CategoryId = category.Id, Category = category };
        var laptop = new Product { Name = "Gaming Laptop", Slug = "gaming-laptop", Price = 1999.99m, StockQuantity = 5, CategoryId = category.Id, Category = category };
        context.Products.AddRange(cable, laptop);

        var cart = new Cart { UserId = user.Id };
        context.Carts.Add(cart);
        context.CartItems.AddRange(
            new CartItem { Cart = cart, CartId = cart.Id, Product = cable, ProductId = cable.Id, Quantity = 3 },
            new CartItem { Cart = cart, CartId = cart.Id, Product = laptop, ProductId = laptop.Id, Quantity = 1 });
        await context.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(context, CurrentUserFor(user.Id), MapperFactory.Create());

        // Act
        var result = await handler.Handle(new CreateOrderCommand { ShippingAddress = "1 Main St", ContactPhone = "+7 900 000-00-00" }, CancellationToken.None);

        // Assert
        var expectedTotal = 3 * 5.99m + 1 * 1999.99m;
        result.TotalAmount.Should().Be(expectedTotal);
        result.Items.Should().HaveCount(2);
        result.ShippingAddress.Should().Be("1 Main St");
        result.ContactPhone.Should().Be("+7 900 000-00-00");
        result.Status.Should().Be("New");

        (await context.Products.FindAsync(cable.Id))!.StockQuantity.Should().Be(97);
        (await context.Products.FindAsync(laptop.Id))!.StockQuantity.Should().Be(4);

        context.CartItems.Should().BeEmpty();
        context.Orders.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenRequestedQuantityExceedsStock()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var (user, category) = SeedUserAndCategory(context);

        var product = new Product { Name = "Gaming Laptop", Slug = "gaming-laptop", Price = 1999.99m, StockQuantity = 2, CategoryId = category.Id, Category = category };
        context.Products.Add(product);

        var cart = new Cart { UserId = user.Id };
        context.Carts.Add(cart);
        context.CartItems.Add(new CartItem { Cart = cart, CartId = cart.Id, Product = product, ProductId = product.Id, Quantity = 5 });
        await context.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(context, CurrentUserFor(user.Id), MapperFactory.Create());

        // Act
        var act = () => handler.Handle(new CreateOrderCommand { ShippingAddress = "1 Main St", ContactPhone = "+7 900 000-00-00" }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(2, "a failed order must not touch stock");
        context.Orders.Should().BeEmpty();
        context.CartItems.Should().ContainSingle("the cart must be left untouched when the order fails");
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenCartIsEmpty()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var (user, _) = SeedUserAndCategory(context);
        context.Carts.Add(new Cart { UserId = user.Id });
        await context.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(context, CurrentUserFor(user.Id), MapperFactory.Create());

        // Act
        var act = () => handler.Handle(new CreateOrderCommand { ShippingAddress = "1 Main St", ContactPhone = "+7 900 000-00-00" }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        context.Orders.Should().BeEmpty();
    }
}
