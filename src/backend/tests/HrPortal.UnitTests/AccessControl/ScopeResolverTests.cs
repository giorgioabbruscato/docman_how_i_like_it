using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure;
using HrPortal.Tenancy;

namespace HrPortal.UnitTests.AccessControl;

public sealed class ScopeResolverTests
{
    private readonly IScopeResolver _resolver = new ScopeResolver();

    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid EmployeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OtherEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid DepartmentId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid OtherDepartmentId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public void Self_AllowsMatchingEmployee()
    {
        var ctx = CreateContext(employeeId: EmployeeId);
        var resource = new ResourceContext(EmployeeId: EmployeeId);

        _resolver.IsInScope(ctx, AccessScope.Self, resource).Should().BeTrue();
    }

    [Fact]
    public void Self_DeniesDifferentEmployee()
    {
        var ctx = CreateContext(employeeId: EmployeeId);
        var resource = new ResourceContext(EmployeeId: OtherEmployeeId);

        _resolver.IsInScope(ctx, AccessScope.Self, resource).Should().BeFalse();
    }

    [Fact]
    public void Department_AllowsMatchingDepartment()
    {
        var ctx = CreateContext(departmentId: DepartmentId);
        var resource = new ResourceContext(DepartmentId: DepartmentId);

        _resolver.IsInScope(ctx, AccessScope.Department, resource).Should().BeTrue();
    }

    [Fact]
    public void Department_DeniesDifferentDepartment()
    {
        var ctx = CreateContext(departmentId: DepartmentId);
        var resource = new ResourceContext(DepartmentId: OtherDepartmentId);

        _resolver.IsInScope(ctx, AccessScope.Department, resource).Should().BeFalse();
    }

    [Fact]
    public void Team_AllowsSameEmployee()
    {
        var ctx = CreateContext(employeeId: EmployeeId, departmentId: DepartmentId);
        var resource = new ResourceContext(EmployeeId: EmployeeId, DepartmentId: OtherDepartmentId);

        _resolver.IsInScope(ctx, AccessScope.Team, resource).Should().BeTrue();
    }

    [Fact]
    public void Team_AllowsSameDepartment()
    {
        var ctx = CreateContext(employeeId: EmployeeId, departmentId: DepartmentId);
        var resource = new ResourceContext(EmployeeId: OtherEmployeeId, DepartmentId: DepartmentId);

        _resolver.IsInScope(ctx, AccessScope.Team, resource).Should().BeTrue();
    }

    [Fact]
    public void Team_DeniesDifferentEmployeeAndDepartment()
    {
        var ctx = CreateContext(employeeId: EmployeeId, departmentId: DepartmentId);
        var resource = new ResourceContext(EmployeeId: OtherEmployeeId, DepartmentId: OtherDepartmentId);

        _resolver.IsInScope(ctx, AccessScope.Team, resource).Should().BeFalse();
    }

    [Fact]
    public void Tenant_AllowsMatchingTenant()
    {
        var ctx = CreateContext();
        var resource = new ResourceContext(TenantId: TenantId);

        _resolver.IsInScope(ctx, AccessScope.Tenant, resource).Should().BeTrue();
    }

    [Fact]
    public void Tenant_DeniesDifferentTenant()
    {
        var ctx = CreateContext();
        var resource = new ResourceContext(TenantId: Guid.NewGuid());

        _resolver.IsInScope(ctx, AccessScope.Tenant, resource).Should().BeFalse();
    }

    [Fact]
    public void All_AllowsPlatformAdmin()
    {
        var ctx = CreateContext(isPlatformAdmin: true);
        var resource = new ResourceContext(TenantId: Guid.NewGuid());

        _resolver.IsInScope(ctx, AccessScope.All, resource).Should().BeTrue();
    }

    [Fact]
    public void All_DeniesNonPlatformAdmin()
    {
        var ctx = CreateContext(isPlatformAdmin: false);
        var resource = new ResourceContext(TenantId: TenantId);

        _resolver.IsInScope(ctx, AccessScope.All, resource).Should().BeFalse();
    }

    private static TenantContext CreateContext(
        Guid? employeeId = null,
        Guid? departmentId = null,
        bool isPlatformAdmin = false) =>
        TenantContext.CreateTenantOnly(TenantId, "demo") with
        {
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            IsPlatformAdmin = isPlatformAdmin
        };
}
