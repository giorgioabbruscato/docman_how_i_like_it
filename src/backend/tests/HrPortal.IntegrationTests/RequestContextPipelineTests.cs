using System.Net;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class RequestContextPipelineTests : IntegrationTestBase
{
    private static readonly Guid PlatformAdminUserId = Guid.Parse("22222222-2222-4222-8222-222222222201");

    public RequestContextPipelineTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetEmployees_ReturnsForbidden_WhenAuthenticatedWithoutTenantAccess()
    {
        var userId = Guid.NewGuid();
        using var client = CreateClient("guest", userId);

        var response = await client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPlatformTenants_ReturnsForbidden_ForNonPlatformAdmin()
    {
        using var client = CreateClient("employee", DemoUsers.Employee, includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/platform/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPlatformTenants_ReturnsOk_ForPlatformAdmin()
    {
        await SeedPlatformAdminAsync(PlatformAdminUserId, "platform.admin@demo.local");

        using var client = CreateClient("admin", PlatformAdminUserId, includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/platform/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPlatformTenants_ReturnsUnauthorized_WhenAnonymous()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/v1/platform/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task SeedPlatformAdminAsync(Guid userId, string email)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();

        var exists = await db.Set<UserProfile>()
            .AnyAsync(p => p.UserId == userId);

        if (exists)
            return;

        await db.Set<UserProfile>().AddAsync(UserProfile.Create(userId, email, isPlatformAdmin: true));
        await db.SaveChangesAsync();
    }
}
