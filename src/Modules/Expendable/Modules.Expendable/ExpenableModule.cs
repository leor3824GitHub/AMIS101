using Asp.Versioning;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Modules;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Modules.Expendable.Contracts.v1.Requests;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FSH.Modules.Expendable;

public class ExpenableModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var services = builder.Services;

        // Register DbContext
        services.AddHeroDbContext<ExpenableDbContext>();

        // Fluent Validation will be auto-discovered
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/v1/expendable")
            .WithTags("Expendable");

        // Product Endpoints
        group.MapPost("/products", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .RequirePermission(ExpenableModuleConstants.Permissions.Products.Create);

        group.MapPut("/products/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update product details")
            .RequirePermission(ExpenableModuleConstants.Permissions.Products.Update);

        group.MapPost("/products/{id:guid}/activate", ActivateProduct)
            .WithName("ActivateProduct")
            .WithSummary("Activate a product")
            .RequirePermission(ExpenableModuleConstants.Permissions.Products.Activate);

        group.MapPost("/products/{id:guid}/deactivate", DeactivateProduct)
            .WithName("DeactivateProduct")
            .WithSummary("Deactivate a product")
            .RequirePermission(ExpenableModuleConstants.Permissions.Products.Deactivate);

        group.MapGet("/products/{id:guid}", GetProduct)
            .WithName("GetProduct")
            .WithSummary("Get product by ID")
            .RequirePermission(ExpenableModuleConstants.Permissions.Products.View);

        group.MapGet("/products", SearchProducts)
            .WithName("SearchProducts")
            .WithSummary("Search products")
            .RequirePermission(ExpenableModuleConstants.Permissions.Products.View);

        // Purchase Order Endpoints
        group.MapPost("/purchases", CreatePurchaseOrder)
            .WithName("CreatePurchaseOrder")
            .WithSummary("Create a new purchase order")
            .RequirePermission(ExpenableModuleConstants.Permissions.Purchases.Create);

        group.MapPost("/purchases/{id:guid}/submit", SubmitPurchaseOrder)
            .WithName("SubmitPurchaseOrder")
            .WithSummary("Submit a purchase order for approval")
            .RequirePermission(ExpenableModuleConstants.Permissions.Purchases.Update);

        group.MapPost("/purchases/{id:guid}/approve", ApprovePurchaseOrder)
            .WithName("ApprovePurchaseOrder")
            .WithSummary("Approve a purchase order")
            .RequirePermission(ExpenableModuleConstants.Permissions.Purchases.Approve);

        group.MapGet("/purchases/{id:guid}", GetPurchase)
            .WithName("GetPurchase")
            .WithSummary("Get purchase order by ID")
            .RequirePermission(ExpenableModuleConstants.Permissions.Purchases.View);

        group.MapGet("/purchases", SearchPurchases)
            .WithName("SearchPurchases")
            .WithSummary("Search purchase orders")
            .RequirePermission(ExpenableModuleConstants.Permissions.Purchases.View);

        // Supply Request Endpoints
        group.MapPost("/supply-requests", CreateSupplyRequest)
            .WithName("CreateSupplyRequest")
            .WithSummary("Create a new supply request")
            .RequirePermission(ExpenableModuleConstants.Permissions.SupplyRequests.Create);

        group.MapPost("/supply-requests/{id:guid}/submit", SubmitSupplyRequest)
            .WithName("SubmitSupplyRequest")
            .WithSummary("Submit a supply request")
            .RequirePermission(ExpenableModuleConstants.Permissions.SupplyRequests.Update);

        group.MapPost("/supply-requests/{id:guid}/approve", ApproveSupplyRequest)
            .WithName("ApproveSupplyRequest")
            .WithSummary("Approve a supply request")
            .RequirePermission(ExpenableModuleConstants.Permissions.SupplyRequests.Approve);

        group.MapGet("/supply-requests/{id:guid}", GetSupplyRequest)
            .WithName("GetSupplyRequest")
            .WithSummary("Get supply request by ID")
            .RequirePermission(ExpenableModuleConstants.Permissions.SupplyRequests.View);

        group.MapGet("/supply-requests", SearchSupplyRequests)
            .WithName("SearchSupplyRequests")
            .WithSummary("Search supply requests")
            .RequirePermission(ExpenableModuleConstants.Permissions.SupplyRequests.View);

        // Shopping Cart Endpoints
        group.MapPost("/cart/get-or-create", GetOrCreateCart)
            .WithName("GetOrCreateCart")
            .WithSummary("Get or create employee shopping cart")
            .RequirePermission(ExpenableModuleConstants.Permissions.ShoppingCarts.Create);

        group.MapPost("/cart/{id:guid}/add-item", AddToCart)
            .WithName("AddToCart")
            .WithSummary("Add item to shopping cart")
            .RequirePermission(ExpenableModuleConstants.Permissions.ShoppingCarts.Edit);

        group.MapGet("/cart/{id:guid}", GetCart)
            .WithName("GetCart")
            .WithSummary("Get shopping cart")
            .RequirePermission(ExpenableModuleConstants.Permissions.ShoppingCarts.View);

        group.MapPost("/cart/{id:guid}/convert-to-request", ConvertCartToRequest)
            .WithName("ConvertCartToRequest")
            .WithSummary("Convert shopping cart to supply request")
            .RequirePermission(ExpenableModuleConstants.Permissions.ShoppingCarts.Convert);
    }

    // Minimal endpoint implementations
    private static async Task<IResult> CreateProduct(
        CreateProductCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/v1/expendable/products/{result.Id}", result);
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var updatedCommand = command with { Id = id };
        var result = await mediator.Send(updatedCommand, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ActivateProduct(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ActivateProductCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeactivateProduct(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeactivateProductCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetProduct(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductQuery(id), cancellationToken);
        return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> SearchProducts(
        string? keyword,
        string? status,
        int pageNumber = 1,
        int pageSize = 10,
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchProductsQuery
        {
            Keyword = keyword,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> CreatePurchaseOrder(
        CreatePurchaseOrderCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/v1/expendable/purchases/{result.Id}", result);
    }

    private static async Task<IResult> SubmitPurchaseOrder(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new SubmitPurchaseOrderCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> ApprovePurchaseOrder(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ApprovePurchaseOrderCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetPurchase(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPurchaseQuery(id), cancellationToken);
        return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> SearchPurchases(
        string? poNumber,
        string? status,
        int pageNumber = 1,
        int pageSize = 10,
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchPurchasesQuery
        {
            PoNumber = poNumber,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> CreateSupplyRequest(
        CreateSupplyRequestCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/v1/expendable/supply-requests/{result.Id}", result);
    }

    private static async Task<IResult> SubmitSupplyRequest(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new SubmitSupplyRequestCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> ApproveSupplyRequest(
        Guid id,
        ApproveSupplyRequestCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var approveCommand = command with { Id = id };
        await mediator.Send(approveCommand, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetSupplyRequest(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSupplyRequestQuery(id), cancellationToken);
        return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> SearchSupplyRequests(
        string? status,
        string? employeeId,
        string? departmentId,
        int pageNumber = 1,
        int pageSize = 10,
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchSupplyRequestsQuery
        {
            Status = status,
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetOrCreateCart(
        GetOrCreateCartCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> AddToCart(
        Guid id,
        AddToCartCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var addCommand = command with { CartId = id };
        await mediator.Send(addCommand, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetCart(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCartQuery(id), cancellationToken);
        return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> ConvertCartToRequest(
        Guid id,
        ConvertCartToSupplyRequestCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var convertCommand = command with { CartId = id };
        var result = await mediator.Send(convertCommand, cancellationToken);
        return TypedResults.Created($"/api/v1/expendable/supply-requests/{result.Id}", result);
    }
}
