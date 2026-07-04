using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.Persistence;

public static class DbInitializer
{
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

        if (!await dbContext.Set<Tenant>().AnyAsync())
        {
            var demoTenant = Tenant.Create("Demo Company", "demo");
            await dbContext.Set<Tenant>().AddAsync(demoTenant);
            await dbContext.SaveChangesAsync();
        }
    }
}
