using FluentValidation;

namespace Store.Application.Features.Users.Queries.GetAdminUsers;

public class GetAdminUsersQueryValidator : AbstractValidator<GetAdminUsersQuery>
{
    private static readonly string[] AllowedSortValues = ["newest", "oldest"];

    public GetAdminUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.SortBy)
            .Must(sortBy => AllowedSortValues.Contains(sortBy))
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortValues)}.");
    }
}
