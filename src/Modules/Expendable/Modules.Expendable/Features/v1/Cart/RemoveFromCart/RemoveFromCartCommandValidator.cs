using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Cart;

namespace FSH.Modules.Expendable.Features.v1.Cart.RemoveFromCart;

public sealed class RemoveFromCartCommandValidator : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
