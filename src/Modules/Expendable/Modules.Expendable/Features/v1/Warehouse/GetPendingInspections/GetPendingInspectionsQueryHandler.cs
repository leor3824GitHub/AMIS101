using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Purchases;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Warehouse.GetPendingInspections;

public sealed class GetPendingInspectionsQueryHandler : IQueryHandler<GetPendingInspectionsQuery, PagedResponse<PurchaseInspectionDto>>
{
    private readonly ExpendableDbContext _dbContext;

    public GetPendingInspectionsQueryHandler(ExpendableDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<PurchaseInspectionDto>> Handle(GetPendingInspectionsQuery query, CancellationToken cancellationToken)
    {
        var inspections = _dbContext.PurchaseInspections
            .Where(pi => pi.Status == InspectionStatus.Pending);

        if (query.WarehouseLocationId.HasValue && query.WarehouseLocationId != Guid.Empty)
            inspections = inspections.Where(pi => pi.WarehouseLocationId == query.WarehouseLocationId);

        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 20;
        var total = await inspections.CountAsync(cancellationToken);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

        var items = await inspections
            .OrderByDescending(pi => pi.InspectionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(i => i.ToPurchaseInspectionDto()).ToList();

        return new PagedResponse<PurchaseInspectionDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }
}
