namespace FSH.Modules.MasterData.Contracts.v1.Categories;

public record CategoryDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive);

public record CreateCategoryCommand(
    string Code,
    string Name,
    string? Description = null);

public record UpdateCategoryCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description = null,
    bool IsActive = true);

public record GetCategoryQuery(Guid Id);

public record DeleteCategoryCommand(Guid Id);
