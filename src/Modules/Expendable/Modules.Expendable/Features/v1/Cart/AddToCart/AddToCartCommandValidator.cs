using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Cart;

namespace FSH.Modules.Expendable.Features.v1.Cart.AddToCart;

public sealed class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");
    }
}
