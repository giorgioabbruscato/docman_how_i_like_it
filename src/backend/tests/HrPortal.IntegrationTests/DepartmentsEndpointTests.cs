using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Departments.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class DepartmentsEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DepartmentsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WithoutAuth()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantSlug);

        var response = await client.GetAsync("/api/v1/departments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/departments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");
        var code = $"D{Guid.NewGuid():N}"[..6].ToUpperInvariant();

        var response = await client.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Engineering",
            code,
            description = "Dev team"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync($"/api/v1/departments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_SetsCreatedBy_FromUserContext()
    {
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        using var client = CreateAuthenticatedClient("hr", userId);
        var code = $"A{Guid.NewGuid():N}"[..6].ToUpperInvariant();

        var response = await client.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Audit Dept",
            code,
            description = "Created by test user"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<DepartmentResponse>(JsonOptions);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var department = await db.Set<Department>()
            .IgnoreQueryFilters()
            .SingleAsync(d => d.Id == body!.Id);

        department.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public async Task Update_ReturnsForbidden_ForEmployeeRole()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var code = $"U{Guid.NewGuid():N}"[..6].ToUpperInvariant();
        var createResponse = await hrClient.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Original",
            code,
            description = "To update"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var department = await createResponse.Content.ReadFromJsonAsync<DepartmentResponse>(JsonOptions);

        using var employeeClient = CreateAuthenticatedClient("employee");
        var response = await employeeClient.PutAsJsonAsync($"/api/v1/departments/{department!.Id}", new
        {
            name = "Updated",
            code,
            description = "Employee attempt"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_ForEmployeeRole()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var code = $"X{Guid.NewGuid():N}"[..6].ToUpperInvariant();
        var createResponse = await hrClient.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "To Delete",
            code,
            description = "Delete test"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var department = await createResponse.Content.ReadFromJsonAsync<DepartmentResponse>(JsonOptions);

        using var employeeClient = CreateAuthenticatedClient("employee");
        var response = await employeeClient.DeleteAsync($"/api/v1/departments/{department!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record DepartmentResponse(Guid Id, string Code);
}
