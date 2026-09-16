using MediatR;
using Store.Application.Features.Products.Dtos;

namespace Store.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto>
{
    public Guid Id { get; set; }
}
