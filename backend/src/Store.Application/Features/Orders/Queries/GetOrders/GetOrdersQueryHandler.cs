using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Common.Models;
using Store.Application.Features.Orders.Dtos;

namespace Store.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PaginatedList<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public Task<PaginatedList<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");

        var query = _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider);

        return PaginatedList<OrderDto>.CreateAsync(query, request.Page, request.PageSize, cancellationToken);
    }
}
