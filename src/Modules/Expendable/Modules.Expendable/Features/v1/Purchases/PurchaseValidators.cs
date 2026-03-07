using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Purchases;

namespace FSH.Modules.Expendable.Features.v1.Purchases;

public sealed class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required")
            .MaximumLength(50).WithMessage("Supplier ID must not exceed 50 characters");
    }
}

public sealed class AddPurchaseLineItemCommandValidator : AbstractValidator<AddPurchaseLineItemCommand>
{
    public AddPurchaseLineItemCommandValidator()
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty().WithMessage("Purchase ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero");
    }
}

public sealed class RecordPurchaseReceiptCommandValidator : AbstractValidator<RecordPurchaseReceiptCommand>
{
    public RecordPurchaseReceiptCommandValidator()
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty().WithMessage("Purchase ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.ReceivedQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Received quantity cannot be negative");

        RuleFor(x => x.RejectedQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Rejected quantity cannot be negative");
    }
}
