using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Expendable.Data;

/// <summary>
/// Initializes the Expendable module database context.
/// Handles migrations and seeding for the expendable business domain.
/// </summary>
internal sealed class ExpendableDbInitializer(
    ILogger<ExpendableDbInitializer> logger,
    ExpendableDbContext context) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[{Tenant}] applied database migrations for expendable module", context.TenantInfo?.Identifier);
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Seed default data if needed in the future
        await Task.CompletedTask;
    }
}
