using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Employees.Domain;
using HrPortal.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.Persistence;

public static class DbInitializer
{
    private const string DemoTenantSlug = "demo";
    private const string DemoEmployeeEmail = "employee@demo.local";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();

        var pending = await dbContext.Database.GetPendingMigrationsAsync();
        var applied = await dbContext.Database.GetAppliedMigrationsAsync();

        if (applied.Any() || pending.Any())
            await dbContext.Database.MigrateAsync();
        else
            await dbContext.Database.EnsureCreatedAsync();

        var demoTenant = await dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Slug == DemoTenantSlug);

        if (demoTenant is null)
        {
            demoTenant = Tenant.Create("Demo Company", DemoTenantSlug);
            await dbContext.Set<Tenant>().AddAsync(demoTenant);
            await dbContext.SaveChangesAsync();
        }

        var demoEmployeeExists = await dbContext.Set<Employee>()
            .AnyAsync(e => e.TenantId == demoTenant.Id && e.Email == DemoEmployeeEmail);

        if (!demoEmployeeExists)
        {
            var demoEmployee = Employee.Create(
                demoTenant.Id,
                "Employee",
                "User",
                DemoEmployeeEmail,
                new DateOnly(2024, 1, 15),
                jobTitle: "Developer");

            await dbContext.Set<Employee>().AddAsync(demoEmployee);
            await dbContext.SaveChangesAsync();
        }
    }
}
