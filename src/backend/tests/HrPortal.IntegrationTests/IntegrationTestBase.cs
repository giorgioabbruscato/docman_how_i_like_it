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

    protected HttpClient CreateAuthenticatedClient(string role, Guid? userId = null)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantSlug);

        if (userId.HasValue)
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId.Value.ToString());

        return client;
    }

    protected static StringContent JsonContent(string json) =>
        new(json, System.Text.Encoding.UTF8, "application/json");

    public void Dispose() => Client.Dispose();
}
