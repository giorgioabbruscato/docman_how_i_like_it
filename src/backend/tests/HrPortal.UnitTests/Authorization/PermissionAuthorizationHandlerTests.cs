using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.Audit.Application;
using HrPortal.Authorization;
using HrPortal.Authorization.Infrastructure;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace HrPortal.UnitTests.Authorization;

public sealed class PermissionAuthorizationHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task HandleRequirementAsync_Succeeds_WhenPolicyEngineAllows()
    {
        var policyEngine = new Mock<IPolicyEngine>();
        var resourceLoader = new Mock<IResourceLoader>();
        var auditService = new Mock<IAuditService>();
        var accessor = new Mock<ITenantContextAccessor>();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();

        var tenantContext = CreateTenantContext();
        var resource = new ResourceContext(EmployeeId: Guid.NewGuid(), TenantId: TenantId);
        var httpContext = new DefaultHttpContext();

        accessor.Setup(a => a.Current).Returns(tenantContext);
        httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);
        resourceLoader
            .Setup(l => l.LoadAsync(httpContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        policyEngine
            .Setup(e => e.Can(tenantContext, Permissions.EmployeeReadTenant, resource))
            .Returns(true);

        var handler = CreateHandler(
            policyEngine.Object,
            resourceLoader.Object,
            auditService.Object,
            accessor.Object,
            httpContextAccessor.Object);

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement(Permissions.EmployeeReadTenant)],
            new System.Security.Claims.ClaimsPrincipal(),
            null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        auditService.Verify(
            a => a.LogAccessDecisionAsync(
                It.Is<AccessDecisionEntry>(e =>
                    e.Allowed &&
                    e.Permission == Permissions.EmployeeReadTenant &&
                    e.ActorUserId == UserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleRequirementAsync_DoesNotSucceed_WhenPolicyEngineDenies()
    {
        var policyEngine = new Mock<IPolicyEngine>();
        var resourceLoader = new Mock<IResourceLoader>();
        var auditService = new Mock<IAuditService>();
        var accessor = new Mock<ITenantContextAccessor>();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();

        var tenantContext = CreateTenantContext();
        var httpContext = new DefaultHttpContext();

        accessor.Setup(a => a.Current).Returns(tenantContext);
        httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);
        resourceLoader
            .Setup(l => l.LoadAsync(httpContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceContext?)null);
        policyEngine
            .Setup(e => e.Can(tenantContext, Permissions.EmployeeDeleteTenant, null))
            .Returns(false);

        var handler = CreateHandler(
            policyEngine.Object,
            resourceLoader.Object,
            auditService.Object,
            accessor.Object,
            httpContextAccessor.Object);

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement(Permissions.EmployeeDeleteTenant)],
            new System.Security.Claims.ClaimsPrincipal(),
            null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        auditService.Verify(
            a => a.LogAccessDecisionAsync(
                It.Is<AccessDecisionEntry>(e =>
                    !e.Allowed &&
                    e.Permission == Permissions.EmployeeDeleteTenant),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static PermissionAuthorizationHandler CreateHandler(
        IPolicyEngine policyEngine,
        IResourceLoader resourceLoader,
        IAuditService auditService,
        ITenantContextAccessor tenantContextAccessor,
        IHttpContextAccessor httpContextAccessor) =>
        new(
            policyEngine,
            resourceLoader,
            auditService,
            tenantContextAccessor,
            httpContextAccessor);

    private static TenantContext CreateTenantContext() =>
        TenantContext.CreateTenantOnly(TenantId, "demo") with
        {
            UserId = UserId,
            Permissions = [Permissions.EmployeeReadTenant]
        };
}
