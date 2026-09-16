namespace Store.Application.Features.Orders.Dtos;

public class AdminOrderDto : OrderDto
{
    public Guid UserId { get; set; }
    public string CustomerEmail { get; set; } = null!;
    public string CustomerFullName { get; set; } = null!;
}
