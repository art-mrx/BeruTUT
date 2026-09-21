using Store.Domain.Common;
using Store.Domain.Enums;

namespace Store.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = null!;
    public string ContactPhone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
