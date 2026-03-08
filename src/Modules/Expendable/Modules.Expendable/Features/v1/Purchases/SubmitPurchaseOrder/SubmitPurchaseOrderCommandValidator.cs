using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Purchases;

namespace FSH.Modules.Expendable.Features.v1.Purchases.SubmitPurchaseOrder;

public sealed class SubmitPurchaseOrderCommandValidator : AbstractValidator<SubmitPurchaseOrderCommand>
{
    public SubmitPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Purchase ID is required");
    }
}
