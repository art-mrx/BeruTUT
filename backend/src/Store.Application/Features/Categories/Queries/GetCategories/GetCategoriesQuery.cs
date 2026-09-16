using MediatR;
using Store.Application.Features.Categories.Dtos;

namespace Store.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}
