using System.Net;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class DocumentsEndpointTests : IntegrationTestBase
{
    public DocumentsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/documents");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync($"/api/v1/documents/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
