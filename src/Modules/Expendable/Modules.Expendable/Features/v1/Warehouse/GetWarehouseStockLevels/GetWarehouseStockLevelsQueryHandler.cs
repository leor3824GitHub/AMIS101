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
        var inventories = _dbContext.ProductInventories.AsNoTracking()
            .Where(pi => pi.WarehouseLocationId == query.WarehouseLocationId)
            .OrderBy(pi => pi.ProductCode);

        var projected = inventories.Select(i => i.ToProductInventoryDto());
        return await projected.ToPagedResponseAsync(query, cancellationToken).ConfigureAwait(false);
    }
}
