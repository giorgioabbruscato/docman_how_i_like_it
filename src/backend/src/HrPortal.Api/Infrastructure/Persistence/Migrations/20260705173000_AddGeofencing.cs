using System;
using HrPortal.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPortal.Api.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(HrPortalDbContext))]
    [Migration("20260705173000_AddGeofencing")]
    public partial class AddGeofencing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MatchedGeofenceZoneId",
                schema: "attendance",
                table: "attendance_sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GpsUnavailableAtCheckIn",
                schema: "attendance",
                table: "attendance_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "geofence_zones",
                schema: "attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    RadiusMeters = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_geofence_zones", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "geofence_settings",
                schema: "attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeofencingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AllowCheckInWithoutGps = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_geofence_settings", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "IX_geofence_zones_TenantId_IsActive",
                schema: "attendance",
                table: "geofence_zones",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_geofence_settings_TenantId",
                schema: "attendance",
                table: "geofence_settings",
                column: "TenantId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "geofence_settings", schema: "attendance");
            migrationBuilder.DropTable(name: "geofence_zones", schema: "attendance");
            migrationBuilder.DropColumn(name: "GpsUnavailableAtCheckIn", schema: "attendance", table: "attendance_sessions");
            migrationBuilder.DropColumn(name: "MatchedGeofenceZoneId", schema: "attendance", table: "attendance_sessions");
        }
    }
}
