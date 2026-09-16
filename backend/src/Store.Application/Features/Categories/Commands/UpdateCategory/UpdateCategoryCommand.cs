using MediatR;
using Store.Application.Features.Categories.Dtos;

namespace Store.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
}
