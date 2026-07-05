using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Employees.Domain;
using HrPortal.IntegrationTests.Infrastructure;
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
        var body = await response.Content.ReadFromJsonAsync<EmployeeIdResponse>(JsonOptions);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var demoTenant = await db.Set<Tenant>().SingleAsync(t => t.Slug == "demo");
        var employee = await db.Set<Employee>()
            .IgnoreQueryFilters()
            .SingleAsync(e => e.Id == body!.Id);

        employee.TenantId.Should().Be(demoTenant.Id);
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

    private sealed record EmployeeIdResponse(Guid Id);
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
