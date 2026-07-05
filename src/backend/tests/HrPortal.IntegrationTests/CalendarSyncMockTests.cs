using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.AccessControl.Domain;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Integrations.Application;
using HrPortal.Integrations.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class CalendarSyncMockTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CalendarSyncMockTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task OAuthCallback_StoresEncryptedTokens()
    {
        using var employeeClient = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var redirectUri = "http://localhost:5173/settings/calendar/callback";

        var connectResponse = await employeeClient.GetAsync(
            $"/api/v1/integrations/calendar/connect/Google?redirectUri={Uri.EscapeDataString(redirectUri)}");
        connectResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var connect = await connectResponse.Content.ReadFromJsonAsync<ConnectResponse>(JsonOptions);
        connect!.AuthorizationUrl.Should().NotBeNullOrWhiteSpace();

        using var callbackClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var callbackUri = new Uri(connect!.AuthorizationUrl);
        var callbackResponse = await callbackClient.GetAsync(callbackUri.PathAndQuery);
        callbackResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var connectionsResponse = await employeeClient.GetAsync("/api/v1/integrations/calendar/connections");
        connectionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var connections = await connectionsResponse.Content.ReadFromJsonAsync<List<ConnectionResponse>>(JsonOptions);
        connections.Should().HaveCount(1);
        connections![0].Provider.Should().Be("Google");

        using var scope = Factory.Services.CreateScope();
        await SetDemoTenantContextAsync(scope);
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var stored = await db.Set<CalendarConnection>()
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.ConnectedAt)
            .FirstAsync();
        stored.AccessTokenEncrypted.Should().NotBe("mock-access-token");
        stored.AccessTokenEncrypted.Should().NotContain("mock-access");
        stored.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task LeaveApproval_TriggersCalendarSync()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId);

        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient, employeeUserId, employeeRoleId, employeeId);

        using var employeeClient = CreateAuthenticatedClient("guest", employeeUserId);
        await ConnectCalendarAsync(employeeClient);

        var createResponse = await employeeClient.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2026-03-10",
            endDate = "2026-03-12",
            type = "Annual"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var managerClient = CreateAuthenticatedClient("guest", managerUserId);
        var approveResponse = await managerClient.PutAsync($"/api/v1/leave-requests/{leave!.Id}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        await SetDemoTenantContextAsync(scope);
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var events = await db.Set<ExternalCalendarEvent>()
            .Where(e => e.LeaveRequestId == leave.Id)
            .ToListAsync();
        events.Should().HaveCount(1);
        events[0].ExternalEventId.Should().StartWith("mock-event-");
    }

    [Fact]
    public async Task ReSync_IsIdempotent_WithSameExternalEventId()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId);

        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient, employeeUserId, employeeRoleId, employeeId);

        using var employeeClient = CreateAuthenticatedClient("guest", employeeUserId);
        await ConnectCalendarAsync(employeeClient);

        var createResponse = await employeeClient.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2026-04-01",
            endDate = "2026-04-03",
            type = "Sick"
        });
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var managerClient = CreateAuthenticatedClient("guest", managerUserId);
        await managerClient.PutAsync($"/api/v1/leave-requests/{leave!.Id}/approve", null);

        using var adminClient = CreateAuthenticatedClient("admin");
        await adminClient.PostAsync($"/api/v1/integrations/calendar/sync/{leave.Id}", null);
        await adminClient.PostAsync($"/api/v1/integrations/calendar/sync/{leave.Id}", null);

        using var scope = Factory.Services.CreateScope();
        await SetDemoTenantContextAsync(scope);
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var events = await db.Set<ExternalCalendarEvent>()
            .Where(e => e.LeaveRequestId == leave.Id)
            .ToListAsync();
        events.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteLeaveEvent_RemovesExternalCalendarEvent()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId);

        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient, employeeUserId, employeeRoleId, employeeId);

        using var employeeClient = CreateAuthenticatedClient("guest", employeeUserId);
        await ConnectCalendarAsync(employeeClient);

        var createResponse = await employeeClient.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2026-05-01",
            endDate = "2026-05-02",
            type = "Personal"
        });
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var managerClient = CreateAuthenticatedClient("guest", managerUserId);
        await managerClient.PutAsync($"/api/v1/leave-requests/{leave!.Id}/approve", null);

        using var scope = Factory.Services.CreateScope();
        await SetDemoTenantContextAsync(scope);
        var syncService = scope.ServiceProvider.GetRequiredService<ICalendarSyncService>();
        var deleteResult = await syncService.DeleteLeaveEventAsync(leave.Id);
        deleteResult.IsSuccess.Should().BeTrue();

        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var events = await db.Set<ExternalCalendarEvent>()
            .Where(e => e.LeaveRequestId == leave.Id)
            .ToListAsync();
        events.Should().BeEmpty();
    }

    private static async Task SetDemoTenantContextAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var tenant = await db.Set<Tenant>().FirstAsync(t => t.Slug == "demo");
        var accessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
        accessor.Set(TenantScopingContext.ForSeeding(tenant.Id));
    }

    private async Task ConnectCalendarAsync(HttpClient employeeClient)
    {
        var redirectUri = "http://localhost:5173/settings/calendar/callback";
        var connectResponse = await employeeClient.GetAsync(
            $"/api/v1/integrations/calendar/connect/Google?redirectUri={Uri.EscapeDataString(redirectUri)}");
        var connect = await connectResponse.Content.ReadFromJsonAsync<ConnectResponse>(JsonOptions);

        using var callbackClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var callbackUri = new Uri(connect!.AuthorizationUrl);
        await callbackClient.GetAsync(callbackUri.PathAndQuery);
    }

    private sealed record ConnectResponse(string AuthorizationUrl);
    private sealed record ConnectionResponse(Guid Id, string Provider, DateTime ConnectedAt, bool IsActive);
    private sealed record LeaveRequestResponse(Guid Id);
}
