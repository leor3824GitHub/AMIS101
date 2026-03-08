using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Warehouse.SearchProductInventory;

public sealed class SearchProductInventoryQueryHandler : IQueryHandler<SearchProductInventoryQuery, PagedResponse<ProductInventoryDto>>
{
    private readonly ExpendableDbContext _dbContext;

    public SearchProductInventoryQueryHandler(ExpendableDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<ProductInventoryDto>> Handle(SearchProductInventoryQuery query, CancellationToken cancellationToken)
    {
        var inventories = _dbContext.ProductInventories.AsQueryable();

        if (query.WarehouseLocationId.HasValue && query.WarehouseLocationId != Guid.Empty)
            inventories = inventories.Where(pi => pi.WarehouseLocationId == query.WarehouseLocationId);

        if (!string.IsNullOrWhiteSpace(query.ProductCode))
            inventories = inventories.Where(pi => pi.ProductCode != null && pi.ProductCode.Contains(query.ProductCode));

        if (!string.IsNullOrWhiteSpace(query.ProductName))
            inventories = inventories.Where(pi => pi.ProductName != null && pi.ProductName.Contains(query.ProductName));

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
