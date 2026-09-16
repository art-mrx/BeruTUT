using MediatR;
using Store.Application.Features.Categories.Dtos;

namespace Store.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQuery : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
}
