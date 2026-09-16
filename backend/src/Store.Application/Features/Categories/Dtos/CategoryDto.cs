namespace Store.Application.Features.Categories.Dtos;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
}
