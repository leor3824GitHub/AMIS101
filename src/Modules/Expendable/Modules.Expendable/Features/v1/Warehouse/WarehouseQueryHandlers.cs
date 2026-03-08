using FSH.Framework.Caching;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Inventory;
using FSH.Modules.Expendable.Domain.Purchases;
using FSH.Modules.Expendable.Domain.Warehouse;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Warehouse;

/// <summary>Get product inventory by product and warehouse</summary>
public sealed class GetProductInventoryQueryHandler : IQueryHandler<GetProductInventoryQuery, ProductInventoryDto?>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICacheService _cache;

    public GetProductInventoryQueryHandler(ExpendableDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<ProductInventoryDto?> Handle(GetProductInventoryQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"inventory:{query.ProductId}:{query.WarehouseLocationId}";
        var cached = await _cache.GetItemAsync<ProductInventoryDto>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        var inventory = await _dbContext.ProductInventories
            .FirstOrDefaultAsync(pi =>
                pi.ProductId == query.ProductId &&
                pi.WarehouseLocationId == query.WarehouseLocationId,
                cancellationToken);

        if (inventory == null) return null;

        var dto = MapToDto(inventory);
        await _cache.SetItemAsync(cacheKey, dto, TimeSpan.FromHours(1), cancellationToken);

        return dto;
    }

    private static ProductInventoryDto MapToDto(Domain.Warehouse.ProductInventory inventory)
    {
        return new ProductInventoryDto(
            inventory.Id,
            inventory.ProductId,
            inventory.ProductCode ?? string.Empty,
            inventory.ProductName ?? string.Empty,
            inventory.WarehouseLocationId,
            inventory.WarehouseLocationName ?? string.Empty,
            inventory.QuantityAvailable,
            inventory.QuantityReserved,
            inventory.QuantityOnHand,
            inventory.QuantityIssued,
            inventory.TotalValue,
            inventory.Status.ToString(),
            inventory.Batches.Select(b => new InventoryBatchDto(
                b.PurchaseId,
                b.ProductId,
                b.QuantityAvailable,
                b.QuantityIssued,
                b.QuantityRemaining,
                b.UnitPrice,
                b.TotalValue,
                b.ReceivedDate,
                b.InspectionDate,
                b.FirstIssueDate,
                b.Version
            )).ToList(),
            inventory.FirstReceiptDate,
            inventory.LastReceiptDate,
            inventory.LastIssueDate
        );
    }
}

/// <summary>Search product inventory across warehouses</summary>
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

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResponse<ProductInventoryDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }

    private static ProductInventoryDto MapToDto(Domain.Warehouse.ProductInventory inventory)
    {
        return new ProductInventoryDto(
            inventory.Id,
            inventory.ProductId,
            inventory.ProductCode ?? string.Empty,
            inventory.ProductName ?? string.Empty,
            inventory.WarehouseLocationId,
            inventory.WarehouseLocationName ?? string.Empty,
            inventory.QuantityAvailable,
            inventory.QuantityReserved,
            inventory.QuantityOnHand,
            inventory.QuantityIssued,
            inventory.TotalValue,
            inventory.Status.ToString(),
            inventory.Batches.Select(b => new InventoryBatchDto(
                b.PurchaseId,
                b.ProductId,
                b.QuantityAvailable,
                b.QuantityIssued,
                b.QuantityRemaining,
                b.UnitPrice,
                b.TotalValue,
                b.ReceivedDate,
                b.InspectionDate,
                b.FirstIssueDate,
                b.Version
            )).ToList(),
            inventory.FirstReceiptDate,
            inventory.LastReceiptDate,
            inventory.LastIssueDate
        );
    }
}

/// <summary>Get warehouse stock levels summary</summary>
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

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResponse<ProductInventoryDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }

    private static ProductInventoryDto MapToDto(Domain.Warehouse.ProductInventory inventory)
    {
        return new ProductInventoryDto(
            inventory.Id,
            inventory.ProductId,
            inventory.ProductCode ?? string.Empty,
            inventory.ProductName ?? string.Empty,
            inventory.WarehouseLocationId,
            inventory.WarehouseLocationName ?? string.Empty,
            inventory.QuantityAvailable,
            inventory.QuantityReserved,
            inventory.QuantityOnHand,
            inventory.QuantityIssued,
            inventory.TotalValue,
            inventory.Status.ToString(),
            inventory.Batches.Select(b => new InventoryBatchDto(
                b.PurchaseId,
                b.ProductId,
                b.QuantityAvailable,
                b.QuantityIssued,
                b.QuantityRemaining,
                b.UnitPrice,
                b.TotalValue,
                b.ReceivedDate,
                b.InspectionDate,
                b.FirstIssueDate,
                b.Version
            )).ToList(),
            inventory.FirstReceiptDate,
            inventory.LastReceiptDate,
            inventory.LastIssueDate
        );
    }
}

/// <summary>Get rejected inventory awaiting disposition</summary>
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

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResponse<RejectedInventoryDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }

    private static RejectedInventoryDto MapToDto(Domain.Inventory.RejectedInventory rejected)
    {
        return new RejectedInventoryDto(
            rejected.Id,
            rejected.PurchaseId,
            rejected.ProductId,
            rejected.ProductCode ?? string.Empty,
            rejected.ProductName ?? string.Empty,
            rejected.WarehouseLocationId,
            rejected.WarehouseLocationName ?? string.Empty,
            rejected.QuantityRejected,
            rejected.UnitPrice,
            rejected.TotalValue,
            rejected.RejectionReason ?? string.Empty,
            rejected.Notes,
            rejected.Status.ToString(),
            rejected.RejectionDate,
            rejected.DispositionDate,
            rejected.DispositionNotes
        );
    }
}

/// <summary>Get inspections pending processing</summary>
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
            .Where(pi => pi.Status == Domain.Purchases.InspectionStatus.Pending);

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

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResponse<PurchaseInspectionDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }

    private static PurchaseInspectionDto MapToDto(Domain.Purchases.PurchaseInspection inspection)
    {
        return new PurchaseInspectionDto(
            inspection.Id,
            inspection.PurchaseId,
            inspection.ProductId,
            inspection.QuantityReceivedForInspection,
            inspection.QuantityAccepted,
            inspection.QuantityRejected,
            inspection.Status.ToString(),
            inspection.RejectionReason ?? string.Empty,
            inspection.Notes,
            inspection.InspectionDate,
            inspection.Defects.Select(d => new InspectionDefectDto(
                d.UnitNumber,
                d.DefectDescription ?? string.Empty,
                d.Severity
            )).ToList()
        );
    }
}


