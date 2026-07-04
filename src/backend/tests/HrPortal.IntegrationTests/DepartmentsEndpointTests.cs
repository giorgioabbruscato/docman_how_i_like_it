using System.Net;
using System.Net.Http.Json;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class DepartmentsEndpointTests : IntegrationTestBase
{
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
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync($"/api/v1/departments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
