using FluentAssertions;
using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure;
using HrPortal.Identity;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using Moq;

namespace HrPortal.UnitTests.Tenancy;

public sealed class TenantContextFactoryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly TenantContext _baseContext;

    public TenantContextFactoryTests()
    {
        _baseContext = TenantContext.CreateTenantOnly(_tenantId, "demo");
    }

    [Fact]
    public async Task EnrichAsync_AnonymousUser_ReturnsBaseContextUnchanged()
    {
        var factory = CreateFactory();

        var result = await factory.EnrichAsync(_baseContext, UserContext.Anonymous);

        result.Should().Be(_baseContext);
        result.UserId.Should().BeNull();
        result.Permissions.Should().BeNull();
    }

    [Fact]
    public async Task EnrichAsync_LegacyKeycloakRolesOnly_MapsPermissionsAndResolves()
    {
        var factory = CreateFactory();
        var userContext = AuthenticatedUser([Roles.Employee]);

        var result = await factory.EnrichAsync(_baseContext, userContext);

        result.IsResolved.Should().BeTrue();
        result.UserId.Should().Be(_userId);
        result.Permissions.Should().Contain(Permissions.EmployeeReadSelf);
        result.RoleSlugs.Should().Contain(SystemRoleTemplates.EmployeeSlug);
    }

    [Fact]
    public async Task EnrichAsync_ActiveMembership_UsesPermissionResolverAndEmployeeId()
    {
        var employeeId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var membership = TenantMembership.Create(_tenantId, _userId, [roleId], employeeId);
        var membershipRepository = new Mock<ITenantMembershipRepository>();
        membershipRepository
            .Setup(r => r.GetActiveByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var permissionResolver = new Mock<IPermissionResolver>();
        permissionResolver
            .Setup(r => r.ResolveAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Permissions.LeaveCreateSelf]);
        permissionResolver
            .Setup(r => r.ResolveRoleSlugsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([SystemRoleTemplates.EmployeeSlug]);

        var factory = CreateFactory(
            membershipRepository: membershipRepository.Object,
            permissionResolver: permissionResolver.Object);

        var result = await factory.EnrichAsync(_baseContext, AuthenticatedUser());

        result.IsResolved.Should().BeTrue();
        result.EmployeeId.Should().Be(employeeId);
        result.Permissions.Should().BeEquivalentTo([Permissions.LeaveCreateSelf]);
        result.RoleSlugs.Should().BeEquivalentTo([SystemRoleTemplates.EmployeeSlug]);
    }

    [Fact]
    public async Task EnrichAsync_PlatformAdmin_AddsElevatedPermissions()
    {
        var profile = UserProfile.Create(_userId, "admin@demo.local", isPlatformAdmin: true);
        var userProfileRepository = new Mock<IUserProfileRepository>();
        userProfileRepository
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var factory = CreateFactory(userProfileRepository: userProfileRepository.Object);
        var result = await factory.EnrichAsync(_baseContext, AuthenticatedUser([Roles.Admin]));

        result.IsPlatformAdmin.Should().BeTrue();
        result.IsResolved.Should().BeTrue();
        result.Permissions.Should().Contain(Permissions.TenantManageAll);
        result.Permissions.Should().Contain(Permissions.BillingManageAll);
    }

    [Fact]
    public async Task EnrichAsync_NoMembershipAndNoLegacyRoles_IsNotResolved()
    {
        var factory = CreateFactory();
        var userContext = AuthenticatedUser(["UnknownRole"]);

        var result = await factory.EnrichAsync(_baseContext, userContext);

        result.IsResolved.Should().BeFalse();
        result.Permissions.Should().BeEquivalentTo([]);
        result.UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task EnrichAsync_MembershipWithDepartmentAttribute_SetsDepartmentId()
    {
        var departmentId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var membership = TenantMembership.Create(
            _tenantId,
            _userId,
            [roleId],
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        var membershipRepository = new Mock<ITenantMembershipRepository>();
        membershipRepository
            .Setup(r => r.GetActiveByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var permissionResolver = new Mock<IPermissionResolver>();
        permissionResolver
            .Setup(r => r.ResolveAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Permissions.EmployeeReadSelf]);
        permissionResolver
            .Setup(r => r.ResolveRoleSlugsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([SystemRoleTemplates.EmployeeSlug]);

        var factory = CreateFactory(
            membershipRepository: membershipRepository.Object,
            permissionResolver: permissionResolver.Object);

        var result = await factory.EnrichAsync(_baseContext, AuthenticatedUser());

        result.DepartmentId.Should().Be(departmentId);
    }

    private UserContext AuthenticatedUser(IReadOnlyList<string>? roles = null) =>
        new()
        {
            UserId = _userId,
            Email = "user@demo.local",
            Roles = roles ?? [],
            IsAuthenticated = true
        };

    private TenantContextFactory CreateFactory(
        ITenantMembershipRepository? membershipRepository = null,
        IUserProfileRepository? userProfileRepository = null,
        ITenantRepository? tenantRepository = null,
        IPermissionResolver? permissionResolver = null)
    {
        var membershipMock = new Mock<ITenantMembershipRepository>();
        membershipMock
            .Setup(r => r.GetActiveByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantMembership?)null);

        var profileMock = new Mock<IUserProfileRepository>();
        profileMock
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var tenantMock = new Mock<ITenantRepository>();
        tenantMock
            .Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tenant.Create("Demo", "demo"));

        var resolverMock = new Mock<IPermissionResolver>();
        resolverMock
            .Setup(r => r.ResolveAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        resolverMock
            .Setup(r => r.ResolveRoleSlugsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        return new TenantContextFactory(
            membershipRepository ?? membershipMock.Object,
            userProfileRepository ?? profileMock.Object,
            tenantRepository ?? tenantMock.Object,
            permissionResolver ?? resolverMock.Object);
    }
}
