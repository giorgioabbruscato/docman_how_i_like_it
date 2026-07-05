using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.SharedKernel.Persistence;
using Microsoft.Extensions.Logging;

namespace HrPortal.AccessControl.Infrastructure.Seeding;

public interface ISystemRoleSeeder
{
    Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

internal sealed class SystemRoleSeeder : ISystemRoleSeeder
{
    private readonly ITenantRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SystemRoleSeeder> _logger;

    public SystemRoleSeeder(
        ITenantRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger<SystemRoleSeeder> logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var created = false;

        foreach (var slug in SystemRoleTemplates.AllSlugs)
        {
            if (await _roleRepository.SlugExistsForTenantAsync(tenantId, slug, cancellationToken))
                continue;

            var role = TenantRole.Create(
                tenantId,
                slug,
                SystemRoleTemplates.GetPermissions(slug),
                isSystem: true);

            await _roleRepository.AddAsync(role, cancellationToken);
            created = true;
            _logger.LogInformation("Seeded system role {RoleSlug} for tenant {TenantId}", slug, tenantId);
        }

        if (created)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
