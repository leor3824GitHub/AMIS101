using Mediator;

namespace FSH.Modules.MasterData.Features.v1.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description = null,
    bool IsActive = true) : ICommand;
