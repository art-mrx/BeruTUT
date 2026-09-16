using FluentValidation;

namespace Store.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    private static readonly string[] AllowedSortValues = ["price_asc", "price_desc", "newest"];

    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy)
            .Must(sortBy => AllowedSortValues.Contains(sortBy))
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortValues)}.");
        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
    }
}
