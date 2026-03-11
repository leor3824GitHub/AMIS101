using FluentValidation;
using FSH.Modules.MasterData.Contracts.v1.References;

namespace FSH.Modules.MasterData.Features.v1.Employees.DeleteEmployee;

public sealed class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee id is required");
    }
}