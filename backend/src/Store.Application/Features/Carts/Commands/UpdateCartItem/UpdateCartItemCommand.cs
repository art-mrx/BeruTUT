using MediatR;
using Store.Application.Features.Carts.Dtos;

namespace Store.Application.Features.Carts.Commands.UpdateCartItem;

public class UpdateCartItemCommand : IRequest<CartDto>
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
}
