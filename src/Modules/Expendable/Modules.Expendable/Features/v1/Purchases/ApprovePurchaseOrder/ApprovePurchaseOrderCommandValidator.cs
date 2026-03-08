using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Purchases;

namespace FSH.Modules.Expendable.Features.v1.Purchases.ApprovePurchaseOrder;

public sealed class ApprovePurchaseOrderCommandValidator : AbstractValidator<ApprovePurchaseOrderCommand>
{
    public ApprovePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Purchase ID is required");
    }
}
