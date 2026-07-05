using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Employees.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class TenantIsolationTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TenantIsolationTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UnresolvedMultiModeContext_DoesNotLeakTenantData()
    {
        using var scope = Factory.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();

        accessor.Current.IsResolved.Should().BeFalse();
        accessor.Current.Mode.Should().Be(TenantDeploymentMode.Multi);

        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var count = await db.Set<Employee>().CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task GetEmployees_ReturnsForbidden_WhenUserHasNoTenantAccess()
    {
        var userId = Guid.NewGuid();
        using var client = CreateClient("guest", userId);

        var response = await client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #region Employees

    [Fact]
    public async Task Employees_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "manager");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "list");

        var listResponse = await tenantBClient.GetAsync("/api/v1/employees");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var employees = await listResponse.Content.ReadFromJsonAsync<List<EmployeeListItem>>(JsonOptions);
        employees.Should().NotContain(e => e.Id == employeeId);
    }

    [Fact]
    public async Task Employees_GetById_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "get");

        var response = await tenantBClient.GetAsync($"/api/v1/employees/{employeeId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Employees_Update_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "update");

        var response = await tenantBClient.PutAsJsonAsync($"/api/v1/employees/{employeeId}", new
        {
            firstName = "Hacked",
            lastName = "User",
            email = "hacked@example.com",
            jobTitle = "Intruder"
        });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Employees_Delete_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "delete");

        var response = await tenantBClient.DeleteAsync($"/api/v1/employees/{employeeId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Departments

    [Fact]
    public async Task Departments_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(tenantAClient);

        var listResponse = await tenantBClient.GetAsync("/api/v1/departments");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var departments = await listResponse.Content.ReadFromJsonAsync<List<IdResponse>>(JsonOptions);
        departments.Should().NotContain(d => d.Id == departmentId);
    }

    [Fact]
    public async Task Departments_GetById_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(tenantAClient);

        var response = await tenantBClient.GetAsync($"/api/v1/departments/{departmentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Departments_Update_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(tenantAClient);

        var response = await tenantBClient.PutAsJsonAsync($"/api/v1/departments/{departmentId}", new
        {
            name = "Hacked Dept",
            code = "HACK01",
            description = "Intrusion attempt"
        });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Leave

    [Fact]
    public async Task LeaveRequests_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "manager");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "leave");
        var leaveId = await TenantIsolationFixture.CreateLeaveRequestAsync(tenantAClient, employeeId);

        var listResponse = await tenantBClient.GetAsync("/api/v1/leave-requests");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var requests = await listResponse.Content.ReadFromJsonAsync<List<IdResponse>>(JsonOptions);
        requests.Should().NotContain(r => r.Id == leaveId);
    }

    [Fact]
    public async Task LeaveRequests_GetById_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "employee");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "leave-get");
        var leaveId = await TenantIsolationFixture.CreateLeaveRequestAsync(tenantAClient, employeeId);

        var response = await tenantBClient.GetAsync($"/api/v1/leave-requests/{leaveId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LeaveRequests_Approve_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "manager");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "leave-approve");
        var leaveId = await TenantIsolationFixture.CreateLeaveRequestAsync(tenantAClient, employeeId);

        var response = await tenantBClient.PutAsync($"/api/v1/leave-requests/{leaveId}/approve", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LeaveRequests_Cancel_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "employee");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "leave-cancel");
        var leaveId = await TenantIsolationFixture.CreateLeaveRequestAsync(tenantAClient, employeeId);

        var response = await tenantBClient.DeleteAsync($"/api/v1/leave-requests/{leaveId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Attendance

    [Fact]
    public async Task Attendance_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "manager");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "attendance");
        var recordId = await TenantIsolationFixture.CheckInAsync(tenantAClient, employeeId);

        var listResponse = await tenantBClient.GetAsync("/api/v1/attendance");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await listResponse.Content.ReadFromJsonAsync<List<IdResponse>>(JsonOptions);
        records.Should().NotContain(r => r.Id == recordId);
    }

    [Fact]
    public async Task Attendance_CheckOut_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "employee");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "attendance-out");

        var response = await tenantBClient.PostAsJsonAsync("/api/v1/attendance/check-out", new
        {
            employeeId
        });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Documents

    [Fact]
    public async Task Documents_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "manager");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "doc");
        var documentId = await TenantIsolationFixture.UploadDocumentAsync(tenantAClient, employeeId);

        var listResponse = await tenantBClient.GetAsync("/api/v1/documents");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var documents = await listResponse.Content.ReadFromJsonAsync<List<IdResponse>>(JsonOptions);
        documents.Should().NotContain(d => d.Id == documentId);
    }

    [Fact]
    public async Task Documents_GetById_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "employee");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "doc-get");
        var documentId = await TenantIsolationFixture.UploadDocumentAsync(tenantAClient, employeeId);

        var response = await tenantBClient.GetAsync($"/api/v1/documents/{documentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Documents_Download_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "employee");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "doc-dl");
        var documentId = await TenantIsolationFixture.UploadDocumentAsync(tenantAClient, employeeId);

        var response = await tenantBClient.GetAsync($"/api/v1/documents/{documentId}/download");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Documents_Delete_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "doc-del");
        var documentId = await TenantIsolationFixture.UploadDocumentAsync(tenantAClient, employeeId);

        var response = await tenantBClient.DeleteAsync($"/api/v1/documents/{documentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    private async Task<TenantIsolationFixture.TwoTenantSetup> CreateTwoTenantsAsync()
    {
        using var setupClient = CreateClient(includeTenantHeader: false);
        return await TenantIsolationFixture.CreateTwoTenantsAsync(setupClient);
    }

    private sealed record EmployeeListItem(Guid Id, string Email);
    private sealed record IdResponse(Guid Id);
}
