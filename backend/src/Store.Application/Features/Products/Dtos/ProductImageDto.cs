namespace Store.Application.Features.Products.Dtos;

public class ProductImageDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }
}
