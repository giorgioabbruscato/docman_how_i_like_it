using System.Net;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.IntegrationTests;

public sealed class EndpointAuthorizationGuardTests : IntegrationTestBase
{
    public EndpointAuthorizationGuardTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public void BusinessControllers_RequireAuthorizationExceptTenants()
    {
        var assembly = typeof(Program).Assembly;
        var controllers = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();

        controllers.Should().NotBeEmpty();

        foreach (var controller in controllers)
        {
            if (controller.Name == nameof(HrPortal.Api.Controllers.V1.TenantsController))
                continue;

            var authorize = controller.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);
            authorize.Should().NotBeEmpty($"{controller.Name} must require authorization");
        }
    }

    [Fact]
    public async Task SecurityHeaders_ArePresentOnResponses()
    {
        var response = await Client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Content-Type-Options").Single().Should().Be("nosniff");
        response.Headers.GetValues("X-Frame-Options").Single().Should().Be("DENY");
    }
}
