using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPortal.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenantPlanFeatureGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModulesJson",
                schema: "platform",
                table: "tenants",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModulesJson",
                schema: "platform",
                table: "tenants");
        }
    }
}
