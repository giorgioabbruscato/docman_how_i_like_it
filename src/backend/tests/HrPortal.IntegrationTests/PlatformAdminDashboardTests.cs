using System.Net;
using System.Net.Http.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class PlatformAdminDashboardTests : IntegrationTestBase
{
    public PlatformAdminDashboardTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Theory]
    [MemberData(nameof(AdminRouteCases))]
    public async Task PlatformAdminRoutes_ReturnOk_ForPlatformAdmin(string route)
    {
        using var client = CreateClient("admin", DemoUsers.PlatformAdmin, includeTenantHeader: false);

        var response = await client.GetAsync(route);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(AdminRouteCases))]
    public async Task PlatformAdminRoutes_ReturnForbidden_ForTenantUser(string route)
    {
        using var client = CreateClient("employee", DemoUsers.Employee, includeTenantHeader: false);

        var response = await client.GetAsync(route);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDashboard_ReturnsMetricsMatchingSeededData()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        await SeedTimeEntryAsync(hrClient);

        using var platformClient = CreateClient("admin", DemoUsers.PlatformAdmin, includeTenantHeader: false);

        var dashboardResponse = await platformClient.GetAsync("/api/v1/platform/admin/dashboard");
        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<PlatformDashboardSummaryDto>();
        dashboard.Should().NotBeNull();
        dashboard!.TotalTenants.Should().BeGreaterThan(0);
        dashboard.TotalEmployees.Should().BeGreaterThan(0);
        dashboard.TotalTimeEntriesLast30Days.Should().BeGreaterThan(0);
        dashboard.LicenseSeatsUsed.Should().Be(dashboard.TotalEmployees);
        dashboard.LicenseSeatsTotal.Should().BeGreaterThan(0);

        var tenantsResponse = await platformClient.GetAsync("/api/v1/platform/admin/tenants");
        tenantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tenants = await tenantsResponse.Content.ReadFromJsonAsync<List<PlatformTenantMetricsDto>>();
        tenants.Should().NotBeNull();
        var demoTenant = tenants!.Single(t => t.Slug == TenantSlug);
        demoTenant.EmployeeCount.Should().BeGreaterThan(0);

        var summaryResponse = await platformClient.GetAsync(
            $"/api/v1/platform/admin/tenants/{demoTenant.TenantId}/summary");
        summaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await summaryResponse.Content.ReadFromJsonAsync<PlatformTenantSummaryDto>();
        summary.Should().NotBeNull();
        summary!.EmployeeCount.Should().Be(demoTenant.EmployeeCount);
        summary.TimeEntriesThisMonth.Should().BeGreaterThan(0);
        summary.StorageUsedBytes.Should().BeNull();
    }

    [Fact]
    public async Task GetTenantSummary_ReturnsNotFound_ForUnknownTenant()
    {
        using var platformClient = CreateClient("admin", DemoUsers.PlatformAdmin, includeTenantHeader: false);

        var response = await platformClient.GetAsync(
            $"/api/v1/platform/admin/tenants/{Guid.NewGuid()}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static IEnumerable<object[]> AdminRouteCases() =>
    [
        ["/api/v1/platform/admin/dashboard"],
        ["/api/v1/platform/admin/tenants"],
        ["/api/v1/platform/admin/usage"]
    ];

    private static async Task SeedTimeEntryAsync(HttpClient hrClient)
    {
        var projectId = await TenantIsolationFixture.CreateProjectAsync(hrClient, "platform-admin");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(
            hrClient,
            departmentId,
            "platform-admin-metrics");

        var userId = Guid.NewGuid();
        var roleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(hrClient, userId, roleId, employeeId);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/time-entries")
        {
            Content = global::System.Net.Http.Json.JsonContent.Create(new
            {
                projectId,
                startTime = DateTime.UtcNow.AddHours(-2),
                endTime = DateTime.UtcNow.AddHours(-1)
            })
        };
        request.Headers.Add("X-Tenant-Id", hrClient.DefaultRequestHeaders.GetValues("X-Tenant-Id").First());
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "guest");
        request.Headers.Add(TestAuthHandler.UserIdHeaderName, userId.ToString());

        var response = await hrClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private sealed record PlatformDashboardSummaryDto(
        int TotalTenants,
        int TotalEmployees,
        int ActiveEmployeesLast30Days,
        int TotalTimeEntriesLast30Days,
        int LicenseSeatsUsed,
        int LicenseSeatsTotal);

    private sealed record PlatformTenantMetricsDto(
        Guid TenantId,
        string Slug,
        string Name,
        int EmployeeCount,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? LastActivityAt);

    private sealed record PlatformTenantSummaryDto(
        Guid TenantId,
        string Slug,
        string Name,
        int EmployeeCount,
        int ActiveProjects,
        int TimeEntriesThisMonth,
        int AttendanceSessionsThisMonth,
        int LeaveRequestsPending,
        long? StorageUsedBytes);
}
