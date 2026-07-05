using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPortal.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceSessionRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_records",
                schema: "attendance");

            migrationBuilder.CreateTable(
                name: "attendance_sessions",
                schema: "attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LatitudeCheckIn = table.Column<double>(type: "double precision", nullable: true),
                    LongitudeCheckIn = table.Column<double>(type: "double precision", nullable: true),
                    LatitudeCheckOut = table.Column<double>(type: "double precision", nullable: true),
                    LongitudeCheckOut = table.Column<double>(type: "double precision", nullable: true),
                    AccuracyCheckIn = table.Column<double>(type: "double precision", nullable: true),
                    AccuracyCheckOut = table.Column<double>(type: "double precision", nullable: true),
                    IPAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    Device = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Browser = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WorkedMinutes = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_sessions_TenantId_EmployeeId_Status",
                schema: "attendance",
                table: "attendance_sessions",
                columns: new[] { "TenantId", "EmployeeId", "Status" },
                unique: true,
                filter: "\"Status\" = 'Open'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_sessions",
                schema: "attendance");

            migrationBuilder.CreateTable(
                name: "attendance_records",
                schema: "attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckIn = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CheckOut = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_records", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_records_TenantId_EmployeeId_Date",
                schema: "attendance",
                table: "attendance_records",
                columns: new[] { "TenantId", "EmployeeId", "Date" },
                unique: true);
        }
    }
}
