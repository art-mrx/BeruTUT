using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Store.Application.Common.Interfaces;
using Store.Application.Common.Models;
using Store.Application.Features.Orders.Dtos;

namespace Store.Application.Features.Orders.Queries.GetAdminOrders;

public class GetAdminOrdersQueryHandler : IRequestHandler<GetAdminOrdersQuery, PaginatedList<AdminOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAdminOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<PaginatedList<AdminOrderDto>> Handle(GetAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders.AsQueryable();

        if (request.Status is not null)
        {
            query = query.Where(o => o.Status == request.Status);
        }

        var projected = query
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<AdminOrderDto>(_mapper.ConfigurationProvider);

        return PaginatedList<AdminOrderDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
