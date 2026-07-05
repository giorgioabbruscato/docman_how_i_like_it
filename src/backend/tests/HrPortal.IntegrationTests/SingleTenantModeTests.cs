using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Attendance.Domain;
using HrPortal.Departments.Domain;
using HrPortal.Documents.Domain;
using HrPortal.Employees.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using HrPortal.Leave.Domain;
using HrPortal.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class SingleTenantModeTests : IClassFixture<SingleTenantWebApplicationFactory>, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly SingleTenantWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SingleTenantModeTests(SingleTenantWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEmployees_Succeeds_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient(includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDepartments_Succeeds_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient(includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/departments");

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLeaveRequests_Succeeds_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("manager", includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/leave-requests");

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAttendance_Succeeds_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("manager", includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/attendance");

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDocuments_Succeeds_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("manager", includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/documents");

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateEmployee_ScopesDataToDefaultTenant_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("hr", includeTenantHeader: false);
        var email = $"single.{Guid.NewGuid():N}@demo.local";

        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Single",
            lastName = "Tenant",
            email,
            hireDate = "2024-01-15"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>(JsonOptions);

        var demoTenantId = await GetDemoTenantIdAsync();
        var employee = await LoadEntityAsync<Employee>(body!.Id);
        employee.TenantId.Should().Be(demoTenantId);
    }

    [Fact]
    public async Task CreateDepartment_ScopesDataToDefaultTenant_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("hr", includeTenantHeader: false);
        var code = $"S{Guid.NewGuid():N}"[..6].ToUpperInvariant();

        var response = await client.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Single Dept",
            code,
            description = "Single tenant dept"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>(JsonOptions);

        var demoTenantId = await GetDemoTenantIdAsync();
        var department = await LoadEntityAsync<Department>(body!.Id);
        department.TenantId.Should().Be(demoTenantId);
    }

    [Fact]
    public async Task CreateLeaveRequest_ScopesDataToDefaultTenant_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("hr", includeTenantHeader: false);
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "single-leave");
        var leaveId = await TenantIsolationFixture.CreateLeaveRequestAsync(client, employeeId);

        var demoTenantId = await GetDemoTenantIdAsync();
        var leaveRequest = await LoadEntityAsync<LeaveRequest>(leaveId);
        leaveRequest.TenantId.Should().Be(demoTenantId);
    }

    [Fact]
    public async Task CheckIn_ScopesDataToDefaultTenant_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("hr", includeTenantHeader: false);
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "single-att");
        var recordId = await TenantIsolationFixture.CheckInAsync(client, employeeId);

        var demoTenantId = await GetDemoTenantIdAsync();
        var record = await LoadEntityAsync<AttendanceRecord>(recordId);
        record.TenantId.Should().Be(demoTenantId);
    }

    [Fact]
    public async Task UploadDocument_ScopesDataToDefaultTenant_WithoutTenantHeader()
    {
        using var client = CreateAuthenticatedClient("hr", includeTenantHeader: false);
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "single-doc");
        var documentId = await TenantIsolationFixture.UploadDocumentAsync(client, employeeId);

        var demoTenantId = await GetDemoTenantIdAsync();
        var document = await LoadEntityAsync<Document>(documentId);
        document.TenantId.Should().Be(demoTenantId);
    }

    [Fact]
    public async Task GetMe_ReturnsEnterpriseEquivalentFeatures_RegardlessOfPersistedPlan()
    {
        // Single-tenant (OSS) deployments always get Enterprise-equivalent features, even though the
        // seeded demo tenant's persisted plan could be anything.
        using var client = CreateAuthenticatedClient("admin", includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>(JsonOptions);
        me!.PlanFeatures.AuditLog.Should().BeTrue();
        me.PlanFeatures.CustomRoles.Should().BeTrue();
        me.PlanFeatures.AdvancedReports.Should().BeTrue();
        me.PlanFeatures.MaxEmployees.Should().Be(int.MaxValue);
    }

    private async Task<Guid> GetDemoTenantIdAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var demoTenant = await db.Set<Tenant>().SingleAsync(t => t.Slug == "demo");
        return demoTenant.Id;
    }

    private async Task<TEntity> LoadEntityAsync<TEntity>(Guid id)
        where TEntity : class
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        return await db.Set<TEntity>()
            .IgnoreQueryFilters()
            .SingleAsync(e => EF.Property<Guid>(e, "Id") == id);
    }

    private HttpClient CreateAuthenticatedClient(
        string role = "hr",
        bool includeTenantHeader = true)
    {
        var client = _factory.CreateClient();

        if (includeTenantHeader)
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
        return client;
    }

    public void Dispose() => _client.Dispose();

    private sealed record IdResponse(Guid Id);

    private sealed record MeResponse(PlanFeaturesResponse PlanFeatures);

    private sealed record PlanFeaturesResponse(
        int MaxEmployees,
        bool CustomRoles,
        bool AuditLog,
        bool AdvancedReports);
}

public sealed class MultiTenantResolutionTests : IClassFixture<HrPortalWebApplicationFactory>, IDisposable
{
    private readonly HrPortalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MultiTenantResolutionTests(HrPortalWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEmployees_ReturnsBadRequest_WhenTenantHeaderMissing()
    {
        var response = await _client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Tenant not specified");
    }

    [Fact]
    public async Task GetEmployees_ReturnsNotFound_WhenTenantSlugInvalid()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "nonexistent-slug");

        var response = await client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Tenant not found");
    }

    public void Dispose() => _client.Dispose();
}
