using System.Net.Http.Headers;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<HrPortalWebApplicationFactory>, IDisposable
{
    protected const string TenantSlug = "demo";

    protected readonly HrPortalWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(HrPortalWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected HttpClient CreateClient(string? role = null, Guid? userId = null, bool includeTenantHeader = true)
    {
        var client = Factory.CreateClient();

        if (includeTenantHeader)
            client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantSlug);

        if (!string.IsNullOrWhiteSpace(role))
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);

            if (userId.HasValue)
                client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId.Value.ToString());
        }

        return client;
    }

    protected HttpClient CreateClientForTenant(
        string tenantSlug,
        string? role = null,
        Guid? userId = null)
    {
        var client = CreateClient(role, userId, includeTenantHeader: false);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantSlug);
        return client;
    }

    protected HttpClient CreateAuthenticatedClient(string role, Guid? userId = null) =>
        CreateClient(role, userId);

    protected static StringContent JsonContent(string json) =>
        new(json, System.Text.Encoding.UTF8, "application/json");

    public void Dispose() => Client.Dispose();
}
