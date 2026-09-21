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

        if (request.CreatedFrom is { } createdFrom)
        {
            var from = AsUtc(createdFrom);
            query = query.Where(o => o.CreatedAt >= from);
        }

        if (request.CreatedTo is { } createdTo)
        {
            var to = AsUtc(createdTo);
            query = query.Where(o => o.CreatedAt < to);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            // The order number shown in the UI is a prefix of the order id; tolerate "№" / "#" typed by the user.
            var term = request.Search.Trim().TrimStart('№', '#').Trim().ToLowerInvariant();
            if (term.Length > 0)
            {
                query = query.Where(o => o.Id.ToString().Contains(term));
            }
        }

        // Id as a tie-breaker keeps pagination stable when several orders share a timestamp.
        var ordered = request.SortBy == "oldest"
            ? query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id)
            : query.OrderByDescending(o => o.CreatedAt).ThenBy(o => o.Id);

        var projected = ordered.ProjectTo<AdminOrderDto>(_mapper.ConfigurationProvider);

        return PaginatedList<AdminOrderDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }

    // Npgsql only accepts Kind=Utc for timestamptz comparisons; query-string dates may arrive as Unspecified.
    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
