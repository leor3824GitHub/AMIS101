using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Warehouse.GetWarehouseStockLevels;

public sealed class GetWarehouseStockLevelsQueryHandler : IQueryHandler<GetWarehouseStockLevelsQuery, PagedResponse<ProductInventoryDto>>
{
    private readonly ExpendableDbContext _dbContext;

    public GetWarehouseStockLevelsQueryHandler(ExpendableDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<ProductInventoryDto>> Handle(GetWarehouseStockLevelsQuery query, CancellationToken cancellationToken)
    {
        var inventories = _dbContext.ProductInventories
            .Where(pi => pi.WarehouseLocationId == query.WarehouseLocationId);

        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 20;
        var total = await inventories.CountAsync(cancellationToken);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

        var items = await inventories
            .OrderBy(pi => pi.ProductCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(i => i.ToProductInventoryDto()).ToList();

        return new PagedResponse<ProductInventoryDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }
}
