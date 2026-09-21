using FluentAssertions;
using Store.Application.Features.Users.Queries.GetAdminUsers;
using Store.Application.Tests.Common;
using Store.Domain.Entities;
using Store.Domain.Enums;
using Store.Infrastructure.Persistence;
using Xunit;

namespace Store.Application.Tests.Features.Users;

public class GetAdminUsersQueryHandlerTests
{
    private static async Task<(StoreDbContext context, User anna, User boris, User admin)> ArrangeAsync()
    {
        var context = TestDbContextFactory.Create();
        var anna = new User
        {
            Id = Guid.Parse("aaaa1111-0000-4000-8000-000000000000"), Email = "anna@example.com", FullName = "Anna Ivanova",
            PasswordHash = "h", CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var boris = new User
        {
            Id = Guid.Parse("bbbb2222-0000-4000-8000-000000000000"), Email = "boris@shop.org", FullName = "Boris Petrov",
            PasswordHash = "h", CreatedAt = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc), IsBlocked = true
        };
        var admin = new User
        {
            Id = Guid.Parse("cccc3333-0000-4000-8000-000000000000"), Email = "admin@store.local", FullName = "Store Admin",
            PasswordHash = "h", Role = UserRole.Admin, CreatedAt = new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc)
        };
        context.Users.AddRange(anna, boris, admin);
        context.Orders.AddRange(
            new Order { UserId = anna.Id, User = anna, ShippingAddress = "x", ContactPhone = "1" },
            new Order { UserId = anna.Id, User = anna, ShippingAddress = "x", ContactPhone = "1" });
        await context.SaveChangesAsync();
        return (context, anna, boris, admin);
    }

    private static GetAdminUsersQueryHandler HandlerFor(StoreDbContext context) => new(context, MapperFactory.Create());

    [Fact]
    public async Task Default_ShouldListEveryoneNewestFirst_WithRoleAndOrdersCount()
    {
        var (context, anna, boris, admin) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminUsersQuery(), CancellationToken.None);

        result.Items.Select(u => u.Id).Should().Equal(admin.Id, boris.Id, anna.Id);
        result.Items.Single(u => u.Id == admin.Id).Role.Should().Be("Admin");
        result.Items.Single(u => u.Id == anna.Id).OrdersCount.Should().Be(2);
        result.Items.Single(u => u.Id == boris.Id).IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task SortByOldest_ShouldReverseOrder()
    {
        var (context, anna, boris, admin) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminUsersQuery { SortBy = "oldest" }, CancellationToken.None);

        result.Items.Select(u => u.Id).Should().Equal(anna.Id, boris.Id, admin.Id);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    public async Task IsBlockedFilter_ShouldSplitBlockedAndActive(bool isBlocked, int expected)
    {
        var (context, _, _, _) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminUsersQuery { IsBlocked = isBlocked }, CancellationToken.None);

        result.Items.Should().HaveCount(expected);
        result.Items.Should().OnlyContain(u => u.IsBlocked == isBlocked);
    }

    [Theory]
    [InlineData("anna")]
    [InlineData("IVANOVA")]
    [InlineData("shop.org")]
    [InlineData("№ bbbb2222")]
    [InlineData("#bbbb")]
    public async Task Search_ShouldMatchEmailNameOrNumber_CaseInsensitively(string search)
    {
        var (context, _, _, _) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminUsersQuery { Search = search }, CancellationToken.None);

        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Search_ShouldCombineWithBlockedFilter()
    {
        var (context, _, _, _) = await ArrangeAsync();
        using var _ = context;

        // "example" matches only anna (boris is @shop.org) — but she is not blocked.
        var result = await HandlerFor(context).Handle(
            new GetAdminUsersQuery { Search = "example", IsBlocked = true }, CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Pagination_ShouldReportTotalCount()
    {
        var (context, _, _, _) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminUsersQuery { PageSize = 2 }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
    }
}
