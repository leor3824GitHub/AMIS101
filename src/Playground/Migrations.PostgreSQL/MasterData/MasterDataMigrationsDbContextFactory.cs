using FSH.Modules.MasterData.Data;
using Microsoft.EntityFrameworkCore.Design;

namespace FSH.Playground.Migrations.PostgreSQL.MasterData;

public sealed class MasterDataMigrationsDbContextFactory : IDesignTimeDbContextFactory<MasterDataDbContext>
{
    public MasterDataDbContext CreateDbContext(string[] args) =>
        new MasterDataDbContextFactory().CreateDbContext(args);
}