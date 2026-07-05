using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPortal.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "integrations");

            migrationBuilder.CreateTable(
                name: "calendar_connections",
                schema: "integrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalCalendarId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AccessTokenEncrypted = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    RefreshTokenEncrypted = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_connections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "external_calendar_events",
                schema: "integrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_calendar_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_sync_logs",
                schema: "integrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_sync_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_connections_TenantId_EmployeeId_IsActive",
                schema: "integrations",
                table: "calendar_connections",
                columns: new[] { "TenantId", "EmployeeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_connections_TenantId_EmployeeId_Provider",
                schema: "integrations",
                table: "calendar_connections",
                columns: new[] { "TenantId", "EmployeeId", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_external_calendar_events_TenantId_LeaveRequestId_Provider",
                schema: "integrations",
                table: "external_calendar_events",
                columns: new[] { "TenantId", "LeaveRequestId", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_calendar_sync_logs_TenantId_LeaveRequestId_CreatedAt",
                schema: "integrations",
                table: "calendar_sync_logs",
                columns: new[] { "TenantId", "LeaveRequestId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_sync_logs_TenantId_Status_NextRetryAt",
                schema: "integrations",
                table: "calendar_sync_logs",
                columns: new[] { "TenantId", "Status", "NextRetryAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "calendar_sync_logs", schema: "integrations");
            migrationBuilder.DropTable(name: "external_calendar_events", schema: "integrations");
            migrationBuilder.DropTable(name: "calendar_connections", schema: "integrations");
        }
    }
}
