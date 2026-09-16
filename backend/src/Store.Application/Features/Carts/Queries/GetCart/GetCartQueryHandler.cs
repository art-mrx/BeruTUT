using AutoMapper;
using MediatR;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Carts.Dtos;

namespace Store.Application.Features.Carts.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetCartQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");
        var cart = await CartAccessor.GetOrCreateCartAsync(_context, userId, cancellationToken);
        return _mapper.Map<CartDto>(cart);
    }
}
