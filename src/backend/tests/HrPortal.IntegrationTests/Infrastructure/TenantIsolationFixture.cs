using System.Net;
using System.Net.Http.Json;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests.Infrastructure;

public static class TenantIsolationFixture
{
    public sealed record TwoTenantSetup(
        string TenantASlug,
        string TenantBSlug);

    public static async Task<TwoTenantSetup> CreateTwoTenantsAsync(HttpClient setupClient)
    {
        var tenantASlug = $"a{Guid.NewGuid():N}"[..10].ToLowerInvariant();
        var tenantBSlug = $"b{Guid.NewGuid():N}"[..10].ToLowerInvariant();

        var tenantAResponse = await setupClient.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Tenant A",
            slug = tenantASlug
        });
        tenantAResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var tenantBResponse = await setupClient.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Tenant B",
            slug = tenantBSlug
        });
        tenantBResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        return new TwoTenantSetup(tenantASlug, tenantBSlug);
    }

    public static async Task<Guid> CreateEmployeeAsync(HttpClient client, string? emailPrefix = null)
    {
        var email = $"{emailPrefix ?? "emp"}.{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Test",
            lastName = "Employee",
            email,
            hireDate = "2024-01-15"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> CreateEmployeeInDepartmentAsync(
        HttpClient client,
        Guid departmentId,
        string? emailPrefix = null)
    {
        var email = $"{emailPrefix ?? "emp"}.{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Test",
            lastName = "Employee",
            email,
            hireDate = "2024-01-15",
            departmentId
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> GetRoleIdBySlugAsync(HttpClient adminClient, string slug)
    {
        var response = await adminClient.GetAsync("/api/v1/roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await response.Content.ReadFromJsonAsync<List<TenantRoleDto>>();
        return roles!.Single(r => r.Slug == slug).Id;
    }

    public static async Task CreateMembershipAsync(
        HttpClient adminClient,
        Guid userId,
        Guid roleId,
        Guid? employeeId = null,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        var response = await adminClient.PostAsJsonAsync("/api/v1/memberships", new
        {
            userId,
            roleIds = new[] { roleId },
            employeeId,
            attributes
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    public static async Task<Guid> CreateDepartmentAsync(HttpClient client)
    {
        var code = $"D{Guid.NewGuid():N}"[..6].ToUpperInvariant();
        var response = await client.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Engineering",
            code,
            description = "Dev team"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> CreateProjectAsync(HttpClient client, string? namePrefix = null)
    {
        var name = $"{namePrefix ?? "project"}-{Guid.NewGuid():N}"[..30];
        var response = await client.PostAsJsonAsync("/api/v1/projects", new
        {
            name,
            status = "Active"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> CreateLeaveRequestAsync(HttpClient client, Guid employeeId)
    {
        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await GetRoleIdBySlugAsync(client, SystemRoleTemplates.EmployeeSlug);
        await CreateMembershipAsync(client, employeeUserId, employeeRoleId, employeeId);

        using var request = CreateEmployeeScopedRequest(
            client,
            HttpMethod.Post,
            "/api/v1/leave-requests",
            employeeUserId,
            JsonContent.Create(new
            {
                employeeId,
                startDate = "2025-07-01",
                endDate = "2025-07-05",
                type = "Annual"
            }));

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> CheckInAsync(HttpClient client, Guid employeeId)
    {
        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await GetRoleIdBySlugAsync(client, SystemRoleTemplates.EmployeeSlug);
        await CreateMembershipAsync(client, employeeUserId, employeeRoleId, employeeId);

        using var request = CreateEmployeeScopedRequest(
            client,
            HttpMethod.Post,
            "/api/v1/attendance/check-in",
            employeeUserId,
            JsonContent.Create(new { employeeId }));

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> UploadDocumentAsync(HttpClient client, Guid employeeId)
    {
        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await GetRoleIdBySlugAsync(client, SystemRoleTemplates.EmployeeSlug);
        await CreateMembershipAsync(client, employeeUserId, employeeRoleId, employeeId);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(employeeId.ToString()), "employeeId");
        content.Add(new StringContent("Contract"), "category");
        var fileContent = new ByteArrayContent("test document content"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "contract.pdf");

        using var request = CreateEmployeeScopedRequest(
            client,
            HttpMethod.Post,
            "/api/v1/documents",
            employeeUserId,
            content);

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    private static HttpRequestMessage CreateEmployeeScopedRequest(
        HttpClient client,
        HttpMethod method,
        string path,
        Guid userId,
        HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, path) { Content = content };
        CopyRequestHeader(client, request, "X-Tenant-Id");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "guest");
        request.Headers.Add(TestAuthHandler.UserIdHeaderName, userId.ToString());
        return request;
    }

    private static void CopyRequestHeader(HttpClient client, HttpRequestMessage request, string headerName)
    {
        if (client.DefaultRequestHeaders.TryGetValues(headerName, out var values))
            request.Headers.TryAddWithoutValidation(headerName, values);
    }

    public sealed record IdResponse(Guid Id);
}
