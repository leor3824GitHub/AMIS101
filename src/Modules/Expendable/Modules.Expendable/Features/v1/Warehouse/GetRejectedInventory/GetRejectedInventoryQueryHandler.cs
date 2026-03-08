using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Warehouse.GetRejectedInventory;

public sealed class GetRejectedInventoryQueryHandler : IQueryHandler<GetRejectedInventoryQuery, PagedResponse<RejectedInventoryDto>>
{
    private readonly ExpendableDbContext _dbContext;

    public GetRejectedInventoryQueryHandler(ExpendableDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<RejectedInventoryDto>> Handle(GetRejectedInventoryQuery query, CancellationToken cancellationToken)
    {
        var rejected = _dbContext.RejectedInventories.AsQueryable();

        if (query.WarehouseLocationId.HasValue && query.WarehouseLocationId != Guid.Empty)
            rejected = rejected.Where(ri => ri.WarehouseLocationId == query.WarehouseLocationId);

        if (!string.IsNullOrWhiteSpace(query.Status))
            rejected = rejected.Where(ri => ri.Status.ToString() == query.Status);

        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 20;
        var total = await rejected.CountAsync(cancellationToken);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

        var items = await rejected
            .OrderByDescending(ri => ri.RejectionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(i => i.ToRejectedInventoryDto()).ToList();

        return new PagedResponse<RejectedInventoryDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }
}
