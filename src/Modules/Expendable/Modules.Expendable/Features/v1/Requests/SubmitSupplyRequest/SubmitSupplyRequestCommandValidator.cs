using FluentValidation;
using FSH.Modules.Expendable.Contracts.v1.Requests;

namespace FSH.Modules.Expendable.Features.v1.Requests.SubmitSupplyRequest;

public sealed class SubmitSupplyRequestCommandValidator : AbstractValidator<SubmitSupplyRequestCommand>
{
    public SubmitSupplyRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Request ID is required");
    }
}
