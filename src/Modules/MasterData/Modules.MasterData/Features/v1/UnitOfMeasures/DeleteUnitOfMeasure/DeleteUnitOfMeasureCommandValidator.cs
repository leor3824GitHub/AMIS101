using FluentValidation;
using FSH.Modules.MasterData.Contracts.v1.References;

namespace FSH.Modules.MasterData.Features.v1.UnitOfMeasures.DeleteUnitOfMeasure;

public sealed class DeleteUnitOfMeasureCommandValidator : AbstractValidator<DeleteUnitOfMeasureCommand>
{
    public DeleteUnitOfMeasureCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Unit of measure id is required");
    }
}