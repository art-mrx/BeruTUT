using FluentAssertions;
using Store.Application.Features.Orders.Queries.GetAdminOrders;
using Store.Application.Tests.Common;
using Store.Domain.Entities;
using Store.Domain.Enums;
using Store.Infrastructure.Persistence;
using Xunit;

namespace Store.Application.Tests.Features.Orders;

public class GetAdminOrdersQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day2 = new(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day3 = new(2026, 9, 3, 12, 0, 0, DateTimeKind.Utc);

    private static async Task<(StoreDbContext context, Order o1, Order o2, Order o3)> ArrangeAsync()
    {
        var context = TestDbContextFactory.Create();
        var user = new User { Email = "buyer@example.com", FullName = "Buyer", PasswordHash = "hash" };
        context.Users.Add(user);

        Order Make(string idPrefix, DateTime createdAt, OrderStatus status) => new()
        {
            Id = Guid.Parse($"{idPrefix}-0000-4000-8000-000000000000"),
            UserId = user.Id, User = user, Status = status, CreatedAt = createdAt,
            ShippingAddress = "1 Main St", ContactPhone = "+7 900 000-00-00"
        };

        var o1 = Make("aaaa1111", Day1, OrderStatus.New);
        var o2 = Make("bbbb2222", Day2, OrderStatus.Approved);
        var o3 = Make("cccc3333", Day3, OrderStatus.New);
        context.Orders.AddRange(o1, o2, o3);
        await context.SaveChangesAsync();
        return (context, o1, o2, o3);
    }

    private static GetAdminOrdersQueryHandler HandlerFor(StoreDbContext context) => new(context, MapperFactory.Create());

    [Fact]
    public async Task Default_ShouldReturnNewestFirst()
    {
        var (context, o1, o2, o3) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminOrdersQuery(), CancellationToken.None);

        result.Items.Select(i => i.Id).Should().Equal(o3.Id, o2.Id, o1.Id);
    }

    [Fact]
    public async Task SortByOldest_ShouldReturnOldestFirst()
    {
        var (context, o1, o2, o3) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminOrdersQuery { SortBy = "oldest" }, CancellationToken.None);

        result.Items.Select(i => i.Id).Should().Equal(o1.Id, o2.Id, o3.Id);
    }

    [Fact]
    public async Task DateRange_ShouldIncludeFromAndExcludeTo()
    {
        var (context, o1, o2, _) = await ArrangeAsync();
        using var _ = context;

        // [Day1 12:00, Day3 12:00) -> o1 (exactly at "from") and o2, but not o3 (exactly at "to").
        var result = await HandlerFor(context).Handle(
            new GetAdminOrdersQuery { CreatedFrom = Day1, CreatedTo = Day3, SortBy = "oldest" }, CancellationToken.None);

        result.Items.Select(i => i.Id).Should().Equal(o1.Id, o2.Id);
    }

    [Fact]
    public async Task OnlyCreatedFrom_ShouldFilterOutEarlierOrders()
    {
        var (context, _, o2, o3) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(
            new GetAdminOrdersQuery { CreatedFrom = Day2, SortBy = "oldest" }, CancellationToken.None);

        result.Items.Select(i => i.Id).Should().Equal(o2.Id, o3.Id);
    }

    [Fact]
    public async Task UnspecifiedKindDates_ShouldBeTreatedAsUtc()
    {
        var (context, _, o2, o3) = await ArrangeAsync();
        using var _ = context;

        var unspecified = DateTime.SpecifyKind(Day2, DateTimeKind.Unspecified);
        var result = await HandlerFor(context).Handle(
            new GetAdminOrdersQuery { CreatedFrom = unspecified, SortBy = "oldest" }, CancellationToken.None);

        result.Items.Select(i => i.Id).Should().Equal(o2.Id, o3.Id);
    }

    [Theory]
    [InlineData("bbbb2222")]
    [InlineData("BBBB2222")]
    [InlineData("№ bbbb2222")]
    [InlineData("#bbbb")]
    [InlineData("  b2222-0000  ")]
    public async Task Search_ShouldMatchOrderNumberFragment_CaseInsensitively(string search)
    {
        var (context, _, o2, _) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminOrdersQuery { Search = search }, CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Id.Should().Be(o2.Id);
    }

    [Fact]
    public async Task Search_WithNoMatch_ShouldReturnEmpty()
    {
        var (context, _, _, _) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(new GetAdminOrdersQuery { Search = "ffffffff" }, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Filters_ShouldCombine()
    {
        var (context, _, _, o3) = await ArrangeAsync();
        using var _ = context;

        // Status New matches o1 and o3; date from Day2 leaves only o3.
        var result = await HandlerFor(context).Handle(
            new GetAdminOrdersQuery { Status = OrderStatus.New, CreatedFrom = Day2 }, CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Id.Should().Be(o3.Id);
    }

    [Fact]
    public async Task Pagination_ShouldReportTotalCountOfFilteredSet()
    {
        var (context, _, _, _) = await ArrangeAsync();
        using var _ = context;

        var result = await HandlerFor(context).Handle(
            new GetAdminOrdersQuery { Page = 1, PageSize = 1, Status = OrderStatus.New }, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(2);
    }
}

public class GetAdminOrdersQueryValidatorTests
{
    private readonly GetAdminOrdersQueryValidator _validator = new();

    [Theory]
    [InlineData("newest", true)]
    [InlineData("oldest", true)]
    [InlineData(null, true)]
    [InlineData("random", false)]
    public void SortBy_ShouldOnlyAllowKnownValues(string? sortBy, bool valid)
        => _validator.Validate(new GetAdminOrdersQuery { SortBy = sortBy }).IsValid.Should().Be(valid);

    [Fact]
    public void CreatedTo_ShouldBeAfterCreatedFrom()
    {
        var from = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc);

        _validator.Validate(new GetAdminOrdersQuery { CreatedFrom = from, CreatedTo = from.AddDays(1) }).IsValid.Should().BeTrue();
        _validator.Validate(new GetAdminOrdersQuery { CreatedFrom = from, CreatedTo = from }).IsValid.Should().BeFalse();
        _validator.Validate(new GetAdminOrdersQuery { CreatedFrom = from, CreatedTo = from.AddDays(-1) }).IsValid.Should().BeFalse();
    }
}
