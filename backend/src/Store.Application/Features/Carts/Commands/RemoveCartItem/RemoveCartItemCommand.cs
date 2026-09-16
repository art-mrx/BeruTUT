using MediatR;

namespace Store.Application.Features.Carts.Commands.RemoveCartItem;

public class RemoveCartItemCommand : IRequest
{
    public Guid ItemId { get; set; }
}
