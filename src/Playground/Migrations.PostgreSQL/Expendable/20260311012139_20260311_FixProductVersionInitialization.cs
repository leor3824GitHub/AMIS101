using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Playground.Migrations.PostgreSQL.Expendable
{
    /// <inheritdoc />
    public partial class _20260311_FixProductVersionInitialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure pgcrypto is available for gen_random_bytes()
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            // Backfill any existing NULL Version values and set a server default
            migrationBuilder.Sql("UPDATE expendable.\"Products\" SET \"Version\" = gen_random_bytes(8) WHERE \"Version\" IS NULL;");
            migrationBuilder.Sql("ALTER TABLE expendable.\"Products\" ALTER COLUMN \"Version\" SET DEFAULT gen_random_bytes(8);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the server default for Version on rollback
            migrationBuilder.Sql("ALTER TABLE expendable.\"Products\" ALTER COLUMN \"Version\" DROP DEFAULT;");
        }
    }
}
