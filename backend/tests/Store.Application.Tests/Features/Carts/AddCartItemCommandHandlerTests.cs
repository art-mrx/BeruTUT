using FluentAssertions;
using Moq;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Carts.Commands.AddCartItem;
using Store.Application.Tests.Common;
using Store.Domain.Entities;
using Xunit;

namespace Store.Application.Tests.Features.Carts;

public class AddCartItemCommandHandlerTests
{
    private static ICurrentUserService CurrentUserFor(Guid userId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(x => x.UserId).Returns(userId);
        return mock.Object;
    }

    [Fact]
    public async Task Handle_ShouldAccumulateQuantity_WhenProductAlreadyInCart()
    {
        using var context = TestDbContextFactory.Create();

        var user = new User { Email = "buyer@example.com", FullName = "Buyer", PasswordHash = "hash" };
        var category = new Category { Name = "Electronics", Slug = "electronics" };
        var product = new Product { Name = "USB Cable", Slug = "usb-cable", Price = 5.99m, StockQuantity = 10, CategoryId = category.Id, Category = category };
        context.Users.Add(user);
        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new AddCartItemCommandHandler(context, CurrentUserFor(user.Id), MapperFactory.Create());

        await handler.Handle(new AddCartItemCommand { ProductId = product.Id, Quantity = 2 }, CancellationToken.None);
        var result = await handler.Handle(new AddCartItemCommand { ProductId = product.Id, Quantity = 3 }, CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.ProductId == product.Id && i.Quantity == 5);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenRequestedQuantityExceedsStock()
    {
        using var context = TestDbContextFactory.Create();

        var user = new User { Email = "buyer@example.com", FullName = "Buyer", PasswordHash = "hash" };
        var category = new Category { Name = "Electronics", Slug = "electronics" };
        var product = new Product { Name = "Gaming Laptop", Slug = "gaming-laptop", Price = 1999.99m, StockQuantity = 2, CategoryId = category.Id, Category = category };
        context.Users.Add(user);
        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new AddCartItemCommandHandler(context, CurrentUserFor(user.Id), MapperFactory.Create());

        var act = () => handler.Handle(new AddCartItemCommand { ProductId = product.Id, Quantity = 3 }, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        context.CartItems.Should().BeEmpty();
    }
}
