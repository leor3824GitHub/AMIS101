using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Purchases.GetPurchasesBySupplier;

public sealed class GetPurchasesBySupplierQueryHandler : IQueryHandler<GetPurchasesBySupplierQuery, PagedResponse<PurchaseDto>>
{
    private readonly ExpendableDbContext _dbContext;

    public GetPurchasesBySupplierQueryHandler(ExpendableDbContext dbContext)
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
