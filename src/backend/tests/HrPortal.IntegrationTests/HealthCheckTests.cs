using System.Net;
using FluentAssertions;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class HealthCheckTests : IClassFixture<HrPortalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(HrPortalWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
