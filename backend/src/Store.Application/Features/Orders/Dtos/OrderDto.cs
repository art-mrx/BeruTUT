namespace Store.Application.Features.Orders.Dtos;

public class OrderDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
