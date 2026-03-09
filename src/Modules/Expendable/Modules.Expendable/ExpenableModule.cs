using Asp.Versioning;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Shared.Persistence;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Modules;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Modules.Expendable.Contracts.v1.Requests;
using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Features.v1.Products.CreateProduct;
using FSH.Modules.Expendable.Features.v1.Products.UpdateProduct;
using FSH.Modules.Expendable.Features.v1.Products.ActivateProduct;
using FSH.Modules.Expendable.Features.v1.Products.DeactivateProduct;
using FSH.Modules.Expendable.Features.v1.Products.DeleteProduct;
using FSH.Modules.Expendable.Features.v1.Products.GetProduct;
using FSH.Modules.Expendable.Features.v1.Products.SearchProducts;
using FSH.Modules.Expendable.Features.v1.Purchases.CreatePurchaseOrder;
using FSH.Modules.Expendable.Features.v1.Purchases.AddPurchaseLineItem;
using FSH.Modules.Expendable.Features.v1.Purchases.RemovePurchaseLineItem;
using FSH.Modules.Expendable.Features.v1.Purchases.SubmitPurchaseOrder;
using FSH.Modules.Expendable.Features.v1.Purchases.ApprovePurchaseOrder;
using FSH.Modules.Expendable.Features.v1.Purchases.RecordPurchaseReceipt;
using FSH.Modules.Expendable.Features.v1.Purchases.CancelPurchaseOrder;
using FSH.Modules.Expendable.Features.v1.Purchases.GetPurchase;
using FSH.Modules.Expendable.Features.v1.Purchases.GetPurchasesBySupplier;
using FSH.Modules.Expendable.Features.v1.Purchases.SearchPurchases;
using FSH.Modules.Expendable.Features.v1.Requests.CreateSupplyRequest;
using FSH.Modules.Expendable.Features.v1.Requests.SubmitSupplyRequest;
using FSH.Modules.Expendable.Features.v1.Requests.ApproveSupplyRequest;
using FSH.Modules.Expendable.Features.v1.Requests.GetSupplyRequest;
using FSH.Modules.Expendable.Features.v1.Requests.SearchSupplyRequests;
using FSH.Modules.Expendable.Features.v1.Requests.RejectSupplyRequest;
using FSH.Modules.Expendable.Features.v1.Requests.GetEmployeeSupplyRequests;
using FSH.Modules.Expendable.Features.v1.Cart.GetOrCreateCart;
using FSH.Modules.Expendable.Features.v1.Cart.AddToCart;
using FSH.Modules.Expendable.Features.v1.Cart.GetCart;
using FSH.Modules.Expendable.Features.v1.Cart.ConvertCartToRequest;
using FSH.Modules.Expendable.Features.v1.Cart.RemoveFromCart;
using FSH.Modules.Expendable.Features.v1.Cart.ClearCart;
using FSH.Modules.Expendable.Features.v1.Warehouse.RecordInspection;
using FSH.Modules.Expendable.Features.v1.Warehouse.ReserveProductInventory;
using FSH.Modules.Expendable.Features.v1.Warehouse.CancelProductInventoryReservation;
using FSH.Modules.Expendable.Features.v1.Warehouse.IssueFromProductInventory;
using FSH.Modules.Expendable.Features.v1.Warehouse.MarkRejectedInventoryReturned;
using FSH.Modules.Expendable.Features.v1.Warehouse.MarkRejectedInventoryDisposed;
using FSH.Modules.Expendable.Features.v1.Warehouse.GetProductInventory;
using FSH.Modules.Expendable.Features.v1.Warehouse.SearchProductInventory;
using FSH.Modules.Expendable.Features.v1.Warehouse.GetWarehouseStockLevels;
using FSH.Modules.Expendable.Features.v1.Warehouse.GetRejectedInventory;
using FSH.Modules.Expendable.Features.v1.Warehouse.GetPendingInspections;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FSH.Modules.Expendable;

public class ExpendableModule : IModule
{
    private static readonly IReadOnlyList<FshPermission> RegisteredPermissions =
    [
        new("View Expendable", "View", "Expendable"),
        new("Create Expendable", "Create", "Expendable"),
        new("Update Expendable", "Update", "Expendable"),
        new("Delete Expendable", "Delete", "Expendable"),

        new("View Expendable Products", "View", "Expendable.Products", IsBasic: true),
        new("Create Expendable Products", "Create", "Expendable.Products"),
        new("Update Expendable Products", "Update", "Expendable.Products"),
        new("Delete Expendable Products", "Delete", "Expendable.Products"),
        new("Activate Expendable Products", "Activate", "Expendable.Products"),
        new("Deactivate Expendable Products", "Deactivate", "Expendable.Products"),

        new("View Expendable Purchases", "View", "Expendable.Purchases", IsBasic: true),
        new("Create Expendable Purchases", "Create", "Expendable.Purchases"),
        new("Update Expendable Purchases", "Update", "Expendable.Purchases"),
        new("Delete Expendable Purchases", "Delete", "Expendable.Purchases"),
        new("Approve Expendable Purchases", "Approve", "Expendable.Purchases"),
        new("Receive Expendable Purchases", "Receive", "Expendable.Purchases"),

        new("View Expendable Supply Requests", "View", "Expendable.SupplyRequests", IsBasic: true),
        new("Create Expendable Supply Requests", "Create", "Expendable.SupplyRequests"),
        new("Update Expendable Supply Requests", "Update", "Expendable.SupplyRequests"),
        new("Delete Expendable Supply Requests", "Delete", "Expendable.SupplyRequests"),
        new("Approve Expendable Supply Requests", "Approve", "Expendable.SupplyRequests"),
        new("Reject Expendable Supply Requests", "Reject", "Expendable.SupplyRequests"),

        new("View Expendable Shopping Carts", "View", "Expendable.ShoppingCarts", IsBasic: true),
        new("Create Expendable Shopping Carts", "Create", "Expendable.ShoppingCarts"),
        new("Edit Expendable Shopping Carts", "Edit", "Expendable.ShoppingCarts"),
        new("Clear Expendable Shopping Carts", "Clear", "Expendable.ShoppingCarts"),
        new("Convert Expendable Shopping Carts", "Convert", "Expendable.ShoppingCarts"),

        new("View Expendable Inventory", "View", "Expendable.Inventory", IsBasic: true),
        new("Receive Expendable Inventory", "Receive", "Expendable.Inventory"),
        new("Consume Expendable Inventory", "Consume", "Expendable.Inventory"),
        new("View Expendable Inventory Reports", "ViewReports", "Expendable.Inventory")
    ];

    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var services = builder.Services;

        // Register module permissions so Identity role seeding can assign them.
        PermissionConstants.Register(RegisteredPermissions);

        // Register DbContext
        services.AddHeroDbContext<ExpendableDbContext>();

        // Register database initializer for multi-tenant migrations and seeding
        services.AddScoped<IDbInitializer, ExpendableDbInitializer>();

        // Register hosted service to initialize core database schema on app startup
        services.AddHostedService<FSH.Modules.Expendable.Provisioning.ExpendableDbInitializerHostedService>();

        // Fluent Validation will be auto-discovered
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Product Endpoints - Vertical Slices
        CreateProductEndpoint.Map(endpoints);
        UpdateProductEndpoint.Map(endpoints);
        ActivateProductEndpoint.Map(endpoints);
        DeactivateProductEndpoint.Map(endpoints);
        DeleteProductEndpoint.Map(endpoints);
        GetProductEndpoint.Map(endpoints);
        SearchProductsEndpoint.Map(endpoints);

        // Purchase Order Endpoints - Vertical Slices
        CreatePurchaseOrderEndpoint.Map(endpoints);
        AddPurchaseLineItemEndpoint.Map(endpoints);
        RemovePurchaseLineItemEndpoint.Map(endpoints);
        SubmitPurchaseOrderEndpoint.Map(endpoints);
        ApprovePurchaseOrderEndpoint.Map(endpoints);
        RecordPurchaseReceiptEndpoint.Map(endpoints);
        CancelPurchaseOrderEndpoint.Map(endpoints);
        GetPurchaseEndpoint.Map(endpoints);
        SearchPurchasesEndpoint.Map(endpoints);
        GetPurchasesBySupplierEndpoint.Map(endpoints);

        // Supply Request Endpoints - Vertical Slices
        CreateSupplyRequestEndpoint.Map(endpoints);
        AddSupplyRequestItemEndpoint.Map(endpoints);
        RemoveSupplyRequestItemEndpoint.Map(endpoints);
        SubmitSupplyRequestEndpoint.Map(endpoints);
        ApproveSupplyRequestEndpoint.Map(endpoints);
        RejectSupplyRequestEndpoint.Map(endpoints);
        GetSupplyRequestEndpoint.Map(endpoints);
        GetEmployeeSupplyRequestsEndpoint.Map(endpoints);
        SearchSupplyRequestsEndpoint.Map(endpoints);

        // Shopping Cart Endpoints - Vertical Slices
        GetOrCreateCartEndpoint.Map(endpoints);
        AddToCartEndpoint.Map(endpoints);
        GetCartEndpoint.Map(endpoints);
        RemoveFromCartEndpoint.Map(endpoints);
        ClearCartEndpoint.Map(endpoints);
        ConvertCartToSupplyRequestEndpoint.Map(endpoints);

        // Warehouse Endpoints - Vertical Slices
        RecordInspectionEndpoint.Map(endpoints);
        ReserveProductInventoryEndpoint.Map(endpoints);
        CancelProductInventoryReservationEndpoint.Map(endpoints);
        IssueFromProductInventoryEndpoint.Map(endpoints);
        MarkRejectedInventoryReturnedEndpoint.Map(endpoints);
        MarkRejectedInventoryDisposedEndpoint.Map(endpoints);
        GetProductInventoryEndpoint.Map(endpoints);
        SearchProductInventoryEndpoint.Map(endpoints);
        GetWarehouseStockLevelsEndpoint.Map(endpoints);
        GetRejectedInventoryEndpoint.Map(endpoints);
        GetPendingInspectionsEndpoint.Map(endpoints);
    }
}
