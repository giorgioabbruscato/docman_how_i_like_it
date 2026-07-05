using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.Employees.Domain;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace HrPortal.Api.Infrastructure.Persistence;

public static class DbInitializer
{
    private const string DemoTenantSlug = "demo";
    private const string DemoEmployeeEmail = "employee@demo.local";

    private static readonly string[] DemoModules =
        ["employees", "departments", "leave", "attendance", "documents"];

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
        var roleSeeder = scope.ServiceProvider.GetRequiredService<ISystemRoleSeeder>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (environment.IsEnvironment("Testing"))
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        else
        {
            var pending = await dbContext.Database.GetPendingMigrationsAsync();
            var applied = await dbContext.Database.GetAppliedMigrationsAsync();

            if (applied.Any() || pending.Any())
                await dbContext.Database.MigrateAsync();
            else
                await dbContext.Database.EnsureCreatedAsync();
        }

        var demoTenant = await dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Slug == DemoTenantSlug);

        if (demoTenant is null)
        {
            demoTenant = Tenant.Create("Demo Company", DemoTenantSlug);
            await dbContext.Set<Tenant>().AddAsync(demoTenant);
            await dbContext.SaveChangesAsync();
        }

        if (demoTenant.GetModules().Count == 0 || demoTenant.GetPlan() != TenantPlan.Enterprise)
        {
            demoTenant.SetModules(DemoModules);
            demoTenant.SetPlan(TenantPlan.Enterprise);
            dbContext.Set<Tenant>().Update(demoTenant);
            await dbContext.SaveChangesAsync();
        }

        tenantContextAccessor.Set(TenantScopingContext.ForSeeding(demoTenant.Id));

        await roleSeeder.SeedAsync(demoTenant.Id);

        var demoEmployee = await dbContext.Set<Employee>()
            .FirstOrDefaultAsync(e => e.TenantId == demoTenant.Id && e.Email == DemoEmployeeEmail);

        if (demoEmployee is null)
        {
            demoEmployee = Employee.Create(
                demoTenant.Id,
                "Employee",
                "User",
                DemoEmployeeEmail,
                new DateOnly(2024, 1, 15),
                jobTitle: "Developer");

            await dbContext.Set<Employee>().AddAsync(demoEmployee);
            await dbContext.SaveChangesAsync();
        }

        await SeedDemoUsersAndMembershipsAsync(dbContext, demoTenant.Id, demoEmployee.Id);
        await EnsureUserProfileAsync(
            dbContext,
            DemoUsers.PlatformAdmin,
            DemoUsers.PlatformAdminEmail,
            isPlatformAdmin: true);
    }

    private static async Task SeedDemoUsersAndMembershipsAsync(
        HrPortalDbContext dbContext,
        Guid tenantId,
        Guid demoEmployeeId)
    {
        var roles = await dbContext.Set<TenantRole>()
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync();

        var roleBySlug = roles.ToDictionary(r => r.Slug, StringComparer.Ordinal);

        await EnsureUserProfileAsync(dbContext, DemoUsers.Admin, DemoUsers.AdminEmail);
        await EnsureUserProfileAsync(dbContext, DemoUsers.Hr, DemoUsers.HrEmail);
        await EnsureUserProfileAsync(dbContext, DemoUsers.Employee, DemoUsers.EmployeeEmail);

        await EnsureMembershipAsync(
            dbContext,
            tenantId,
            DemoUsers.Admin,
            [roleBySlug[SystemRoleTemplates.AdminSlug].Id]);

        await EnsureMembershipAsync(
            dbContext,
            tenantId,
            DemoUsers.Hr,
            [roleBySlug[SystemRoleTemplates.HrSlug].Id]);

        await EnsureMembershipAsync(
            dbContext,
            tenantId,
            DemoUsers.Employee,
            [roleBySlug[SystemRoleTemplates.EmployeeSlug].Id],
            demoEmployeeId);
    }

    private static async Task EnsureUserProfileAsync(
        HrPortalDbContext dbContext,
        Guid userId,
        string email,
        bool isPlatformAdmin = false)
    {
        var exists = await dbContext.Set<UserProfile>()
            .AnyAsync(p => p.UserId == userId);

        if (exists)
            return;

        await dbContext.Set<UserProfile>().AddAsync(UserProfile.Create(userId, email, isPlatformAdmin));
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureMembershipAsync(
        HrPortalDbContext dbContext,
        Guid tenantId,
        Guid userId,
        IReadOnlyList<Guid> roleIds,
        Guid? employeeId = null)
    {
        var exists = await dbContext.Set<TenantMembership>()
            .AnyAsync(m => m.TenantId == tenantId && m.UserId == userId && m.IsActive);

        if (exists)
            return;

        var membership = TenantMembership.Create(tenantId, userId, roleIds, employeeId);
        await dbContext.Set<TenantMembership>().AddAsync(membership);
        await dbContext.SaveChangesAsync();
    }
}
