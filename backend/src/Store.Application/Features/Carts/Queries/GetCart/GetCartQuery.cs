using MediatR;
using Store.Application.Features.Carts.Dtos;

namespace Store.Application.Features.Carts.Queries.GetCart;

public class GetCartQuery : IRequest<CartDto>
{
}
