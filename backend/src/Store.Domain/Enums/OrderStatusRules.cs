namespace Store.Domain.Enums;

public static class OrderStatusRules
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> Transitions =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.New] = [OrderStatus.Approved, OrderStatus.Cancelled],
            [OrderStatus.Approved] = [OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Shipped] = [OrderStatus.Delivered],
            [OrderStatus.Delivered] = [],
            [OrderStatus.Cancelled] = []
        };

    public static IReadOnlyList<OrderStatus> GetNext(OrderStatus from) => Transitions[from];

    public static bool CanTransition(OrderStatus from, OrderStatus to) => Transitions[from].Contains(to);
}
