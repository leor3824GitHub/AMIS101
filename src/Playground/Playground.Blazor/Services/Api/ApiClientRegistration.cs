using System.Net.Http;
using FSH.Playground.Blazor.ApiClient;
using FSH.Playground.Blazor.Services.Api.Expendable;
using FSH.Playground.Blazor.Services.Api;

namespace FSH.Playground.Blazor;

internal static class ApiClientRegistration
{
    public static IServiceCollection AddApiClients(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var apiBaseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("Api:BaseUrl configuration is missing.");

        var apiUri = new Uri(apiBaseUrl);

        static HttpClientHandler CreateHandler(Uri apiUri, IWebHostEnvironment environment)
        {
            var handler = new HttpClientHandler();

            // Local development convenience: allow self-signed localhost certs.
            if (environment.IsDevelopment() &&
                (string.Equals(apiUri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(apiUri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)))
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        }

        static HttpClient ResolveClient(IServiceProvider sp) =>
            sp.GetRequiredService<HttpClient>();

        // Register a named HttpClient for token operations (no auth handler to avoid circular dependency)
        services.AddHttpClient("TokenClient", client =>
        {
            client.BaseAddress = apiUri;
        })
        .ConfigurePrimaryHttpMessageHandler(() => CreateHandler(apiUri, environment));

        // TokenClient uses the named HttpClient without the AuthorizationHeaderHandler
        // This avoids circular dependency: TokenRefreshService -> ITokenClient -> HttpClient -> AuthorizationHeaderHandler -> TokenRefreshService
        services.AddTransient<ITokenClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("TokenClient");
            return new TokenClient(client);
        });

        services.AddTransient<IIdentityClient>(sp =>
            new IdentityClient(ResolveClient(sp)));

        services.AddTransient<IAuditsClient>(sp =>
            new AuditsClient(ResolveClient(sp)));

        services.AddTransient<ITenantsClient>(sp =>
            new TenantsClient(ResolveClient(sp)));

        services.AddTransient<IUsersClient>(sp =>
            new UsersClient(ResolveClient(sp)));

        services.AddTransient<IGroupsClient>(sp =>
            new GroupsClient(ResolveClient(sp)));

        services.AddTransient<ISessionsClient>(sp =>
            new SessionsClient(ResolveClient(sp)));

        services.AddTransient<IV1Client>(sp =>
            new V1Client(ResolveClient(sp)));

        // Expendable module clients (feature-first wrappers)
        services.AddTransient<IExpendableClient>(sp =>
            new ExpendableClient(ResolveClient(sp)));

        services.AddTransient<IProductsClient>(sp =>
            new ProductsClient(ResolveClient(sp)));

        services.AddTransient<IExpendableProductsClient, ExpendableProductsClient>();

        services.AddTransient<IPurchasesClient>(sp =>
            new PurchasesClient(ResolveClient(sp)));

        services.AddTransient<IExpendablePurchasesClient>(sp =>
            new ExpendablePurchasesClient(ResolveClient(sp), sp.GetRequiredService<IPurchasesClient>()));

        services.AddTransient<ISupply_requestsClient>(sp =>
            new Supply_requestsClient(ResolveClient(sp)));

        services.AddTransient<IExpendableSupplyRequestsClient, ExpendableSupplyRequestsClient>();

        services.AddTransient<ICartClient>(sp =>
            new CartClient(ResolveClient(sp)));

        services.AddTransient<IExpendableCartClient, ExpendableCartClient>();

        services.AddTransient<IWarehouseClient>(sp =>
            new WarehouseClient(ResolveClient(sp)));

        services.AddTransient<IInventoryClient>(sp =>
            new InventoryClient(ResolveClient(sp)));

        services.AddTransient<IRejectedClient>(sp =>
            new RejectedClient(ResolveClient(sp)));

        services.AddTransient<IExpendableWarehouseClient, ExpendableWarehouseClient>();

        services.AddScoped<IHealthClient>(sp =>
            new HealthClient(ResolveClient(sp)));

        return services;
    }
}
