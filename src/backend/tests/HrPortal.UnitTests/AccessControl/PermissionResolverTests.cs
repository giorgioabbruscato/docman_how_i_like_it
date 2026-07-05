using FluentAssertions;
using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure;
using Moq;

namespace HrPortal.UnitTests.AccessControl;

public sealed class PermissionResolverTests
{
    [Fact]
    public async Task ResolveAsync_UnionsPermissionsFromMultipleRoles()
    {
        var tenantId = Guid.NewGuid();
        var roleA = TenantRole.Create(tenantId, "a", ["employee.read:self"], isSystem: false);
        var roleB = TenantRole.Create(tenantId, "b", ["leave.create:self"], isSystem: false);

        var repository = new Mock<ITenantRoleRepository>();
        repository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([roleA, roleB]);

        var resolver = new PermissionResolver(repository.Object);
        var permissions = await resolver.ResolveAsync([roleA.Id, roleB.Id]);

        permissions.Should().BeEquivalentTo(["employee.read:self", "leave.create:self"]);
    }
}
