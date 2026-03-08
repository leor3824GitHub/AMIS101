using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Products;

namespace FSH.Modules.Expendable.Features.v1.Products.ActivateProduct;

public sealed class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
