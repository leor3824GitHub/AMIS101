using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Eventing.Inbox;
using FSH.Framework.Eventing.Outbox;
using FSH.Framework.Persistence.Context;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Domain.Cart;
using FSH.Modules.Expendable.Domain.Inventory;
using FSH.Modules.Expendable.Domain.Products;
using FSH.Modules.Expendable.Domain.Purchases;
using FSH.Modules.Expendable.Domain.Requests;
using FSH.Modules.Expendable.Domain.Warehouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Expendable.Data;

public class ExpendableDbContext : BaseDbContext
{

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    // Product Management
    public DbSet<Product> Products => Set<Product>();

    // Purchase Orders
    public DbSet<Purchase> Purchases => Set<Purchase>();

    // Supply Requests
    public DbSet<SupplyRequest> SupplyRequests => Set<SupplyRequest>();

    // Shopping Cart
    public DbSet<EmployeeShoppingCart> ShoppingCarts => Set<EmployeeShoppingCart>();

    // Inventory
    public DbSet<EmployeeInventory> EmployeeInventories => Set<EmployeeInventory>();
    public DbSet<InventoryConsumption> InventoryConsumptions => Set<InventoryConsumption>();

    // Warehouse Operations
    public DbSet<ProductInventory> ProductInventories => Set<ProductInventory>();
    public DbSet<PurchaseInspection> PurchaseInspections => Set<PurchaseInspection>();
    public DbSet<RejectedInventory> RejectedInventories => Set<RejectedInventory>();

    public ExpendableDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<ExpendableDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment) : base(multiTenantContextAccessor, options, settings, environment)
    {
        ArgumentNullException.ThrowIfNull(multiTenantContextAccessor);
        ArgumentNullException.ThrowIfNull(settings);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpendableDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration(ExpendableModuleConstants.SchemaName));
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration(ExpendableModuleConstants.SchemaName));
    }
}


