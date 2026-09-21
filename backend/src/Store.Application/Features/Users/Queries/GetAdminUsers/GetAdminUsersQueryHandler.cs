using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Store.Application.Common.Interfaces;
using Store.Application.Common.Models;
using Store.Application.Features.Users.Dtos;

namespace Store.Application.Features.Users.Queries.GetAdminUsers;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, PaginatedList<AdminUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAdminUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<PaginatedList<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        if (request.IsBlocked is not null)
        {
            query = query.Where(u => u.IsBlocked == request.IsBlocked);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            // The "number" shown in the UI is a prefix of the user id; tolerate "№" / "#" typed by the admin.
            var term = request.Search.Trim().TrimStart('№', '#').Trim().ToLowerInvariant();
            if (term.Length > 0)
            {
                query = query.Where(u =>
                    u.Email.ToLower().Contains(term) ||
                    u.FullName.ToLower().Contains(term) ||
                    u.Id.ToString().Contains(term));
            }
        }

        var ordered = request.SortBy == "oldest"
            ? query.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id)
            : query.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id);

        var projected = ordered.ProjectTo<AdminUserDto>(_mapper.ConfigurationProvider);

        return PaginatedList<AdminUserDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
