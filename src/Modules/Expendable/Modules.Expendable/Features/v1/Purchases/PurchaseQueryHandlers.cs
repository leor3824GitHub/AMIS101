using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Purchases;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Purchases;

public sealed class GetPurchaseQueryHandler : IQueryHandler<GetPurchaseQuery, PurchaseDto?>
{
    private readonly ExpenableDbContext _dbContext;

    public GetPurchaseQueryHandler(ExpenableDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PurchaseDto?> Handle(GetPurchaseQuery query, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken)
            .ConfigureAwait(false);
        return purchase?.ToPurchaseDto();
    }
}

public sealed class SearchPurchasesQueryHandler : IQueryHandler<SearchPurchasesQuery, PagedResponse<PurchaseDto>>
{
    private readonly ExpenableDbContext _dbContext;

    public SearchPurchasesQueryHandler(ExpenableDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<PurchaseDto>> Handle(SearchPurchasesQuery query, CancellationToken cancellationToken)
    {
        var purchaseQuery = _dbContext.Purchases.AsNoTracking();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.PoNumber))
        {
            purchaseQuery = purchaseQuery.Where(p => p.PurchaseOrderNumber.Contains(query.PoNumber));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<PurchaseStatus>(query.Status, out var status))
        {
            purchaseQuery = purchaseQuery.Where(p => p.Status == status);
        }

        purchaseQuery = purchaseQuery.OrderByDescending(p => p.OrderDate);

        var projected = purchaseQuery.Select(p => p.ToPurchaseDto());
        return await projected.ToPagedResponseAsync(query, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class GetPurchasesBySupplierQueryHandler : IQueryHandler<GetPurchasesBySupplierQuery, PagedResponse<PurchaseDto>>
{
    private readonly ExpenableDbContext _dbContext;

    public GetPurchasesBySupplierQueryHandler(ExpenableDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<PagedResponse<PurchaseDto>> Handle(GetPurchasesBySupplierQuery query, CancellationToken cancellationToken)
    {
        var purchaseQuery = _dbContext.Purchases.AsNoTracking()
            .Where(p => p.SupplierId == query.SupplierId)
            .OrderByDescending(p => p.OrderDate);

        var projected = purchaseQuery.Select(p => p.ToPurchaseDto());
        return await projected.ToPagedResponseAsync(query, cancellationToken).ConfigureAwait(false);
    }
}
