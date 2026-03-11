using FluentValidation;
using FSH.Modules.MasterData.Contracts.v1.References;

namespace FSH.Modules.MasterData.Features.v1.Departments.DeleteDepartment;

public sealed class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department id is required");
    }
}