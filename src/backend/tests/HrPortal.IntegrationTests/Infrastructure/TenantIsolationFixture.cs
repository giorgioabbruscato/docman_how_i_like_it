using System.Net;
using System.Net.Http.Json;

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

    public static async Task<Guid> CreateLeaveRequestAsync(HttpClient client, Guid employeeId)
    {
        var response = await client.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2025-07-01",
            endDate = "2025-07-05",
            type = "Annual"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> CheckInAsync(HttpClient client, Guid employeeId)
    {
        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            employeeId
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public static async Task<Guid> UploadDocumentAsync(HttpClient client, Guid employeeId)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(employeeId.ToString()), "employeeId");
        content.Add(new StringContent("Contract"), "category");
        var fileContent = new ByteArrayContent("test document content"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "contract.pdf");

        var response = await client.PostAsync("/api/v1/documents", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    public sealed record IdResponse(Guid Id);
}
