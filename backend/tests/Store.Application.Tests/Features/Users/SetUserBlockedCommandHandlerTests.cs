using FluentAssertions;
using Store.Application.Common.Exceptions;
using Store.Application.Features.Users.Commands.SetUserBlocked;
using Store.Application.Tests.Common;
using Store.Domain.Entities;
using Store.Domain.Enums;
using Store.Infrastructure.Persistence;
using Xunit;

namespace Store.Application.Tests.Features.Users;

public class SetUserBlockedCommandHandlerTests
{
    private static SetUserBlockedCommandHandler HandlerFor(StoreDbContext context) => new(context, MapperFactory.Create());

    private static async Task<(StoreDbContext context, User customer)> ArrangeCustomerAsync(int activeTokens = 2)
    {
        var context = TestDbContextFactory.Create();
        var customer = new User { Email = "c@example.com", FullName = "Customer", PasswordHash = "h" };
        context.Users.Add(customer);
        for (var i = 0; i < activeTokens; i++)
        {
            context.RefreshTokens.Add(new RefreshToken { UserId = customer.Id, TokenHash = $"active-{i}", ExpiresAt = DateTime.UtcNow.AddDays(7) });
        }
        // An already-revoked token must keep its original revocation time.
        context.RefreshTokens.Add(new RefreshToken { UserId = customer.Id, TokenHash = "old", ExpiresAt = DateTime.UtcNow.AddDays(7), RevokedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        await context.SaveChangesAsync();
        return (context, customer);
    }

    [Fact]
    public async Task Block_ShouldMarkUserBlocked_AndRevokeAllActiveSessions()
    {
        var (context, customer) = await ArrangeCustomerAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new SetUserBlockedCommand { Id = customer.Id, IsBlocked = true }, CancellationToken.None);

        result.IsBlocked.Should().BeTrue();
        result.BlockedAt.Should().NotBeNull();
        context.RefreshTokens.Where(rt => rt.UserId == customer.Id && rt.TokenHash.StartsWith("active"))
            .Should().OnlyContain(rt => rt.RevokedAt != null);
        context.RefreshTokens.Single(rt => rt.TokenHash == "old").RevokedAt
            .Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Block_Twice_ShouldKeepOriginalBlockedAt()
    {
        var (context, customer) = await ArrangeCustomerAsync(activeTokens: 0);
        using var _ = context;
        var handler = HandlerFor(context);
        var command = new SetUserBlockedCommand { Id = customer.Id, IsBlocked = true };

        var first = await handler.Handle(command, CancellationToken.None);
        await Task.Delay(20);
        var second = await handler.Handle(command, CancellationToken.None);

        second.IsBlocked.Should().BeTrue();
        second.BlockedAt.Should().Be(first.BlockedAt);
    }

    [Fact]
    public async Task Unblock_ShouldClearBlockedState()
    {
        var (context, customer) = await ArrangeCustomerAsync(activeTokens: 0);
        using var _ = context;
        var handler = HandlerFor(context);
        await handler.Handle(new SetUserBlockedCommand { Id = customer.Id, IsBlocked = true }, CancellationToken.None);

        var result = await handler.Handle(new SetUserBlockedCommand { Id = customer.Id, IsBlocked = false }, CancellationToken.None);

        result.IsBlocked.Should().BeFalse();
        result.BlockedAt.Should().BeNull();
    }

    [Fact]
    public async Task Block_Admin_ShouldThrowConflict_AndChangeNothing()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { Email = "a@example.com", FullName = "Admin", PasswordHash = "h", Role = UserRole.Admin };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var act = () => HandlerFor(context).Handle(new SetUserBlockedCommand { Id = admin.Id, IsBlocked = true }, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        context.Users.Single().IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task UnknownUser_ShouldThrowNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var act = () => HandlerFor(context).Handle(new SetUserBlockedCommand { Id = Guid.NewGuid(), IsBlocked = true }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Result_ShouldIncludeOrdersCount()
    {
        var (context, customer) = await ArrangeCustomerAsync(activeTokens: 0);
        using var _ = context;
        for (var i = 0; i < 3; i++)
        {
            context.Orders.Add(new Order { UserId = customer.Id, User = customer, ShippingAddress = "x", ContactPhone = "1" });
        }
        await context.SaveChangesAsync();

        var result = await HandlerFor(context).Handle(new SetUserBlockedCommand { Id = customer.Id, IsBlocked = true }, CancellationToken.None);

        result.OrdersCount.Should().Be(3);
    }
}
