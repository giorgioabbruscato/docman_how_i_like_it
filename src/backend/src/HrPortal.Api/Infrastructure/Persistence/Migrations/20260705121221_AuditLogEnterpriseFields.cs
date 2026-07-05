using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPortal.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogEnterpriseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorEmail",
                schema: "platform",
                table: "audit_logs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Decision",
                schema: "platform",
                table: "audit_logs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                schema: "platform",
                table: "audit_logs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                schema: "platform",
                table: "audit_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetId",
                schema: "platform",
                table: "audit_logs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TenantId_Action",
                schema: "platform",
                table: "audit_logs",
                columns: new[] { "TenantId", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TenantId_Decision",
                schema: "platform",
                table: "audit_logs",
                columns: new[] { "TenantId", "Decision" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_logs_TenantId_Action",
                schema: "platform",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_TenantId_Decision",
                schema: "platform",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "ActorEmail",
                schema: "platform",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "Decision",
                schema: "platform",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                schema: "platform",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "platform",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "TargetId",
                schema: "platform",
                table: "audit_logs");
        }
    }
}
