using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Employees.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class EmployeesEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EmployeesEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WithoutAuth()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantSlug);

        var response = await client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");
        var email = $"mario.{Guid.NewGuid():N}@demo.local";

        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Mario",
            lastName = "Rossi",
            email,
            hireDate = "2024-01-15"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        body!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync($"/api/v1/employees/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_SetsCreatedBy_FromUserContext()
    {
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        using var client = CreateAuthenticatedClient("hr", userId);
        var email = $"audit.{Guid.NewGuid():N}@demo.local";

        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Audit",
            lastName = "User",
            email,
            hireDate = "2024-01-15"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var employee = await db.Set<Employee>()
            .IgnoreQueryFilters()
            .SingleAsync(e => e.Id == body!.Id);

        employee.CreatedBy.Should().Be(userId);
    }

    private sealed record EmployeeResponse(Guid Id, string Email);
}
