using FluentValidation;

namespace Store.Application.Features.Carts.Commands.UpdateCartItem;

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
