using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal interface IExpendableSupplyRequestsClient
{
    Task CreateAsync(CreateSupplyRequestCommand command, CancellationToken cancellationToken = default);

    Task<PagedResponse<SupplyRequestDto>> SearchAsync(
        string? status = null,
        string? employeeId = null,
        string? departmentId = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<SupplyRequestDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SubmitAsync(Guid id, CancellationToken cancellationToken = default);

    Task ApproveAsync(Guid id, ApproveSupplyRequestCommand command, CancellationToken cancellationToken = default);

    public record SupplyRequestDto(
        Guid Id,
        string RequestNumber,
        string Status,
        string DepartmentId,
        string BusinessJustification,
        DateTime CreatedDate,
        int ItemCount);
}
