using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Expendable.Contracts.v1.Requests;

public record SupplyRequestItemDto(
    Guid ProductId,
    int RequestedQuantity,
    int ApprovedQuantity,
    int FulfilledQuantity,
    string? Notes);

public record SupplyRequestDto(
    Guid Id,
    string RequestNumber,
    string EmployeeId,
    string DepartmentId,
    DateTimeOffset RequestDate,
    DateTimeOffset? NeededByDate,
    string Status,
    string? BusinessJustification,
    string? RejectionReason,
    string? ApprovedBy,
    DateTimeOffset? ApprovedOnUtc,
    List<SupplyRequestItemDto> Items,
    DateTimeOffset CreatedOnUtc,
    string? CreatedBy);

public record CreateSupplyRequestCommand(
    string DepartmentId,
    string? BusinessJustification = null,
    DateTimeOffset? NeededByDate = null) : ICommand<SupplyRequestDto>;

public record AddSupplyRequestItemCommand(
    Guid RequestId,
    Guid ProductId,
    int Quantity,
    string? Notes = null) : ICommand<Unit>;

public record RemoveSupplyRequestItemCommand(
    Guid RequestId,
    Guid ProductId) : ICommand<Unit>;

public record SubmitSupplyRequestCommand(Guid Id) : ICommand<Unit>;

public record ApproveSupplyRequestCommand(
    Guid Id,
    Dictionary<Guid, int> ApprovedQuantities) : ICommand<Unit>;

public record RejectSupplyRequestCommand(
    Guid Id,
    string? Reason = null) : ICommand<Unit>;

public record MarkSupplyRequestFulfilledCommand(Guid Id) : ICommand<Unit>;

public record CancelSupplyRequestCommand(Guid Id) : ICommand<Unit>;

public record GetSupplyRequestQuery(Guid Id) : IQuery<SupplyRequestDto?>;

public sealed class SearchSupplyRequestsQuery : IPagedQuery, IQuery<PagedResponse<SupplyRequestDto>>
{
    public string? Status { get; set; }
    public string? EmployeeId { get; set; }
    public string? DepartmentId { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
}

public sealed class GetEmployeeSupplyRequestsQuery : IPagedQuery, IQuery<PagedResponse<SupplyRequestDto>>
{
    public string? EmployeeId { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
}
