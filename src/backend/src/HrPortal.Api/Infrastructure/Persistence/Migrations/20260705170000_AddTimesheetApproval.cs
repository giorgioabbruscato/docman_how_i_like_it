using System;
using HrPortal.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPortal.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(HrPortalDbContext))]
    [Migration("20260705170000_AddTimesheetApproval")]
    public partial class AddTimesheetApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "timesheet_submissions",
                schema: "time_tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalWorkedMinutes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheet_submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "timesheet_submission_entries",
                schema: "time_tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimesheetSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheet_submission_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_timesheet_submission_entries_timesheet_submissions_Timeshee~",
                        column: x => x.TimesheetSubmissionId,
                        principalSchema: "time_tracking",
                        principalTable: "timesheet_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "timesheet_approvals",
                schema: "time_tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimesheetSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheet_approvals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_approvals_TenantId_TimesheetSubmissionId",
                schema: "time_tracking",
                table: "timesheet_approvals",
                columns: new[] { "TenantId", "TimesheetSubmissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_submission_entries_TenantId_TimeEntryId",
                schema: "time_tracking",
                table: "timesheet_submission_entries",
                columns: new[] { "TenantId", "TimeEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_submission_entries_TenantId_TimesheetSubmissionId",
                schema: "time_tracking",
                table: "timesheet_submission_entries",
                columns: new[] { "TenantId", "TimesheetSubmissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_submissions_TenantId_EmployeeId",
                schema: "time_tracking",
                table: "timesheet_submissions",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_submissions_TenantId_EmployeeId_PeriodStart_Period~",
                schema: "time_tracking",
                table: "timesheet_submissions",
                columns: new[] { "TenantId", "EmployeeId", "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_submissions_TenantId_Status",
                schema: "time_tracking",
                table: "timesheet_submissions",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "timesheet_approvals", schema: "time_tracking");
            migrationBuilder.DropTable(name: "timesheet_submission_entries", schema: "time_tracking");
            migrationBuilder.DropTable(name: "timesheet_submissions", schema: "time_tracking");
        }
    }
}
