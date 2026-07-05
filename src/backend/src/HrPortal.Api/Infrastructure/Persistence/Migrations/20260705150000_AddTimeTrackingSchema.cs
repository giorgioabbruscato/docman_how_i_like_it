using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPortal.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeTrackingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "time_tracking");

            migrationBuilder.CreateTable(
                name: "time_entries",
                schema: "time_tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkedMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Billable = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_TenantId_EmployeeId",
                schema: "time_tracking",
                table: "time_entries",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_TenantId_EmployeeId_StartTime",
                schema: "time_tracking",
                table: "time_entries",
                columns: new[] { "TenantId", "EmployeeId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_TenantId_ProjectId",
                schema: "time_tracking",
                table: "time_entries",
                columns: new[] { "TenantId", "ProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "time_entries",
                schema: "time_tracking");
        }
    }
}
