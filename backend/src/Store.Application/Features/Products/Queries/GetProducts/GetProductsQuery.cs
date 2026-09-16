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
}
