using MediatR;
using Store.Application.Common.Models;
using Store.Application.Features.Users.Dtos;

namespace Store.Application.Features.Users.Queries.GetAdminUsers;

public class GetAdminUsersQuery : IRequest<PaginatedList<AdminUserDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>Fragment of the email, full name or user number (id).</summary>
    public string? Search { get; set; }

    /// <summary>true = only blocked, false = only active, null = everyone.</summary>
    public bool? IsBlocked { get; set; }

    /// <summary>Registration date order: "newest" (default) or "oldest".</summary>
    public string? SortBy { get; set; }
}
