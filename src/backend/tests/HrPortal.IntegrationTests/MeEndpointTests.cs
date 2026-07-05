using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class MeEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MeEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMe_WithoutAuth_Returns401()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantSlug);

        var response = await client.GetAsync("/api/v1/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithDemoEmployee_ReturnsPermissionsAndContext()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync("/api/v1/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await response.Content.ReadFromJsonAsync<MeDto>(JsonOptions);
        me.Should().NotBeNull();
        me!.UserId.Should().Be(DemoUsers.Employee);
        me.TenantSlug.Should().Be(TenantSlug);
        me.RoleSlugs.Should().Contain("employee");
        me.Permissions.Should().NotBeEmpty();
        me.Permissions.Should().Contain("employee.read:self");
        me.EmployeeId.Should().NotBeNull();
        me.Features.Should().NotBeEmpty();
        me.IsPlatformAdmin.Should().BeFalse();
    }
}
