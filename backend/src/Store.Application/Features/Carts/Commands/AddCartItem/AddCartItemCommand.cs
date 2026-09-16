using MediatR;
using Store.Application.Features.Carts.Dtos;

namespace Store.Application.Features.Carts.Commands.AddCartItem;

public class AddCartItemCommand : IRequest<CartDto>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
