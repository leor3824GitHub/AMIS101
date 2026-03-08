using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Products;

namespace FSH.Modules.Expendable.Features.v1.Products.DeleteProduct;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
