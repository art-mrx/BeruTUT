using MediatR;
using Store.Application.Features.Categories.Dtos;

namespace Store.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
}
