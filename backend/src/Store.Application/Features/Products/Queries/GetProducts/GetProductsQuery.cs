using MediatR;
using Store.Application.Common.Models;
using Store.Application.Features.Products.Dtos;

namespace Store.Application.Features.Products.Queries.GetProducts;

public class GetProductsQuery : IRequest<PaginatedList<ProductDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Only honored by the Admin-only endpoint (AdminProductsController), which forces this
    /// to true regardless of client input. The public products endpoint never sets it.
    /// </summary>
    public bool IncludeInactive { get; set; }
}
