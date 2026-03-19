using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Cart;

namespace FSH.Modules.Expendable.Features.v1.Cart;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");
    }
}

