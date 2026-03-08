using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Cart;

namespace FSH.Modules.Expendable.Features.v1.Cart.ClearCart;

public sealed class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");
    }
}
