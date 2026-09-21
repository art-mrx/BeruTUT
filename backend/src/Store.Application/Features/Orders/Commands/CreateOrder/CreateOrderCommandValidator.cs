using FluentValidation;

namespace Store.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ContactPhone)
            .NotEmpty()
            .MaximumLength(32)
            .Matches(@"^\+?[0-9\s\-()]{6,32}$")
            .WithMessage("Enter a valid phone number.");
    }
}
