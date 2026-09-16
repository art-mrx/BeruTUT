using FluentValidation;

namespace Store.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(500);
    }
}
