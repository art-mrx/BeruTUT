using MediatR;
using Store.Application.Features.Products.Dtos;

namespace Store.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<ProductDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; }
}
