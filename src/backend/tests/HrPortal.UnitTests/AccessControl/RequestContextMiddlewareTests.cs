using FluentAssertions;
using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure;
using HrPortal.Identity;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using HrPortal.Tenancy.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace HrPortal.UnitTests.AccessControl;

public sealed class RequestContextMiddlewareTests
{
    private readonly Guid _userId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");

    [Fact]
    public async Task InvokeAsync_ExcludedPath_SkipsResolution()
    {
        var (middleware, context, nextCalled) = CreateMiddleware();
        context.Request.Path = "/health";

        await InvokeAsync(middleware, context);

        nextCalled.Value.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_MultiMode_UnresolvedAuthenticatedUser_Returns403()
    {
        var tenant = CreateActiveTenant("demo");
        var (middleware, context, nextCalled) = CreateMiddleware(
            tenant: tenant,
            userContext: AuthenticatedUser(["guest"]),
            enrichResult: TenantContext.CreateTenantOnly(tenant.Id, "demo") with
            {
                UserId = _userId,
                IsResolved = false
            });

        context.Request.Path = "/api/v1/employees";
        context.Request.Headers["X-Tenant-Id"] = "demo";

        await InvokeAsync(middleware, context);

        nextCalled.Value.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_MultiMode_LegacyEmployeeRole_AllowsRequest()
    {
        var tenant = CreateActiveTenant("demo");
        TenantContext? captured = null;
        var accessor = new CapturingTenantContextAccessor(ctx => captured = ctx);

        var (middleware, context, nextCalled) = CreateMiddleware(
            tenant: tenant,
            accessor: accessor,
            userContext: AuthenticatedUser([Roles.Employee]),
            enrichResult: TenantContext.CreateTenantOnly(tenant.Id, "demo") with
            {
                UserId = _userId,
                IsResolved = true,
                Permissions = ["employee.read:self"]
            });

        context.Request.Path = "/api/v1/employees";
        context.Request.Headers["X-Tenant-Id"] = "demo";

        await InvokeAsync(middleware, context);

        nextCalled.Value.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SuspendedTenant_Returns404()
    {
        var tenant = CreateActiveTenant("demo");
        tenant.Suspend();

        var (middleware, context, nextCalled) = CreateMiddleware(tenant: tenant);
        context.Request.Path = "/api/v1/employees";
        context.Request.Headers["X-Tenant-Id"] = "demo";

        await InvokeAsync(middleware, context);

        nextCalled.Value.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task InvokeAsync_PlatformPath_NonPlatformAdmin_Returns403()
    {
        var profileRepo = new Mock<IUserProfileRepository>();
        profileRepo
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserProfile.Create(_userId, "user@demo.local"));

        var (middleware, context, nextCalled) = CreateMiddleware(
            userContext: AuthenticatedUser([Roles.Admin]),
            userProfileRepository: profileRepo.Object);

        context.Request.Path = "/api/v1/platform/tenants";

        await InvokeAsync(middleware, context);

        nextCalled.Value.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_PlatformPath_PlatformAdmin_SetsContextAndContinues()
    {
        var profileRepo = new Mock<IUserProfileRepository>();
        profileRepo
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserProfile.Create(_userId, "admin@platform.local", isPlatformAdmin: true));

        TenantContext? captured = null;
        var accessor = new CapturingTenantContextAccessor(ctx => captured = ctx);

        var (middleware, context, nextCalled) = CreateMiddleware(
            accessor: accessor,
            userContext: AuthenticatedUser([Roles.Admin]),
            userProfileRepository: profileRepo.Object);

        context.Request.Path = "/api/v1/platform/tenants";

        await InvokeAsync(middleware, context);

        nextCalled.Value.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.IsPlatformAdmin.Should().BeTrue();
        captured.UserId.Should().Be(_userId);
        captured.IsResolved.Should().BeTrue();
    }

    private (RequestContextMiddleware Middleware, DefaultHttpContext Context, CapturedBool NextCalled) CreateMiddleware(
        Tenant? tenant = null,
        ITenantContextAccessor? accessor = null,
        UserContext? userContext = null,
        TenantContext? enrichResult = null,
        IUserProfileRepository? userProfileRepository = null)
    {
        var nextCalled = new CapturedBool();
        var middleware = new RequestContextMiddleware(
            _ =>
            {
                nextCalled.Value = true;
                return Task.CompletedTask;
            },
            NullLogger<RequestContextMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var tenantResolver = new Mock<ITenantResolver>();
        tenantResolver
            .Setup(r => r.ResolveSlugAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HttpContext ctx, CancellationToken _) =>
                ctx.Request.Headers["X-Tenant-Id"].FirstOrDefault());

        var tenantRepository = new Mock<ITenantRepository>();
        tenantRepository
            .Setup(r => r.GetBySlugAsync("demo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant ?? CreateActiveTenant("demo"));

        var factory = new Mock<ITenantContextFactory>();
        factory
            .Setup(f => f.EnrichAsync(It.IsAny<TenantContext>(), It.IsAny<UserContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantContext baseContext, UserContext _, CancellationToken _) =>
                enrichResult ?? baseContext);

        var profileRepo = userProfileRepository ?? new Mock<IUserProfileRepository>().Object;

        MiddlewareState = new MiddlewareDependencies(
            tenantResolver.Object,
            tenantRepository.Object,
            accessor ?? new CapturingTenantContextAccessor(_ => { }),
            factory.Object,
            profileRepo,
            userContext ?? UserContext.Anonymous,
            Options.Create(new TenantResolverOptions { Mode = TenantDeploymentMode.Multi }));

        return (middleware, context, nextCalled);
    }

    private MiddlewareDependencies? MiddlewareState { get; set; }

    private Task InvokeAsync(RequestContextMiddleware middleware, HttpContext context)
    {
        var deps = MiddlewareState!;
        return middleware.InvokeAsync(
            context,
            deps.TenantResolver,
            deps.TenantRepository,
            deps.Accessor,
            deps.Factory,
            deps.UserProfileRepository,
            deps.UserContext,
            deps.Options);
    }

    private UserContext AuthenticatedUser(IReadOnlyList<string> roles) =>
        new()
        {
            UserId = _userId,
            Email = "user@demo.local",
            Roles = roles,
            IsAuthenticated = true
        };

    private static Tenant CreateActiveTenant(string slug) =>
        Tenant.Create("Demo", slug);

    private sealed class CapturingTenantContextAccessor : ITenantContextAccessor
    {
        private readonly Action<TenantContext> _onSet;

        public CapturingTenantContextAccessor(Action<TenantContext> onSet) => _onSet = onSet;

        public TenantContext Current { get; private set; } = TenantContext.Empty;

        public void Set(TenantContext context)
        {
            Current = context;
            _onSet(context);
        }
    }

    private sealed class CapturedBool
    {
        public bool Value { get; set; }
    }

    private sealed record MiddlewareDependencies(
        ITenantResolver TenantResolver,
        ITenantRepository TenantRepository,
        ITenantContextAccessor Accessor,
        ITenantContextFactory Factory,
        IUserProfileRepository UserProfileRepository,
        UserContext UserContext,
        IOptions<TenantResolverOptions> Options);
}
