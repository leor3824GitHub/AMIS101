using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.Framework.Eventing.Inbox;
using FSH.Framework.Eventing.Outbox;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Domain.Cart;
using FSH.Modules.Expendable.Domain.Inventory;
using FSH.Modules.Expendable.Domain.Products;
using FSH.Modules.Expendable.Domain.Purchases;
using FSH.Modules.Expendable.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Expendable.Data;

public class ExpenableDbContext : MultiTenantDbContext
{
    private readonly DatabaseOptions _settings;
    private new AppTenantInfo TenantInfo { get; set; }
    private readonly IHostEnvironment _environment;

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

    public ExpenableDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<ExpenableDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment) : base(multiTenantContextAccessor, options)
    {
        ArgumentNullException.ThrowIfNull(multiTenantContextAccessor);
        ArgumentNullException.ThrowIfNull(settings);

        _environment = environment;
        _settings = settings.Value;
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo!;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ExpenableDbContext).Assembly);

        builder.ApplyConfiguration(new OutboxMessageConfiguration(ExpenableModuleConstants.SchemaName));
        builder.ApplyConfiguration(new InboxMessageConfiguration(ExpenableModuleConstants.SchemaName));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!string.IsNullOrWhiteSpace(TenantInfo?.ConnectionString))
        {
            optionsBuilder.ConfigureHeroDatabase(
                _settings.Provider,
                TenantInfo.ConnectionString,
                _settings.MigrationsAssembly,
                _environment.IsDevelopment());
        }
    }
}
