using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Purchases;

namespace FSH.Modules.Expendable.Features.v1.Purchases.CancelPurchaseOrder;

public sealed class CancelPurchaseOrderCommandValidator : AbstractValidator<CancelPurchaseOrderCommand>
{
    public CancelPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Purchase ID is required");
    }
}
