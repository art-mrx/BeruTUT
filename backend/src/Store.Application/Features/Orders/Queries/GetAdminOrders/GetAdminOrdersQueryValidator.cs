using FluentValidation;

namespace Store.Application.Features.Orders.Queries.GetAdminOrders;

public class GetAdminOrdersQueryValidator : AbstractValidator<GetAdminOrdersQuery>
{
    public GetAdminOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}
