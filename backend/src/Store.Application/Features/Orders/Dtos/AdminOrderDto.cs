using Store.Domain.Enums;

namespace Store.Application.Features.Orders.Dtos;

public class AdminOrderDto : OrderDto
{
    public Guid UserId { get; set; }
    public string CustomerEmail { get; set; } = null!;
    public string CustomerFullName { get; set; } = null!;

    // Single source of truth for which actions the admin UI may offer.
    public IReadOnlyList<string> AllowedNextStatuses =>
        Enum.TryParse<OrderStatus>(Status, out var current)
            ? OrderStatusRules.GetNext(current).Select(s => s.ToString()).ToList()
            : [];
}
