using FluentValidation;

namespace Store.Application.Features.Orders.Queries.GetAdminOrders;

public class GetAdminOrdersQueryValidator : AbstractValidator<GetAdminOrdersQuery>
{
    private static readonly string[] AllowedSortValues = ["newest", "oldest"];

    public GetAdminOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Search).MaximumLength(50);
        RuleFor(x => x.SortBy)
            .Must(sortBy => AllowedSortValues.Contains(sortBy))
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortValues)}.");
        RuleFor(x => x.CreatedTo)
            .GreaterThan(x => x.CreatedFrom)
            .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue)
            .WithMessage("CreatedTo must be later than CreatedFrom.");
    }
}
