using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Playground.Migrations.PostgreSQL.Expendable
{
    public partial class AddProductVariants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentProductId",
                schema: "expendable",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                schema: "expendable",
                table: "Products",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_ParentProductId",
                schema: "expendable",
                table: "Products",
                columns: new[] { "TenantId", "ParentProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Products_ParentProductId",
                schema: "expendable",
                table: "Products",
                column: "ParentProductId",
                principalSchema: "expendable",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Products_ParentProductId",
                schema: "expendable",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_ParentProductId",
                schema: "expendable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ParentProductId",
                schema: "expendable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VariantName",
                schema: "expendable",
                table: "Products");
        }
    }
}
