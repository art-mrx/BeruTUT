using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Store.Application.Common.Interfaces;
using Store.Application.Common.Models;
using Store.Application.Features.Products.Dtos;

namespace Store.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = request.IncludeInactive
            ? _context.Products.AsQueryable()
            : _context.Products.Where(p => p.IsActive);

        if (request.CategoryId is not null)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        query = request.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var projected = query.ProjectTo<ProductDto>(_mapper.ConfigurationProvider);

        return PaginatedList<ProductDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
