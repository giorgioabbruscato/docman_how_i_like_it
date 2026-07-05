using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Domain;
using HrPortal.Audit.Application;
using HrPortal.Identity;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using Microsoft.Extensions.Logging;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class TenantRoleService : ITenantRoleService
{
    private readonly ITenantRoleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly UserContext _userContext;
    private readonly IAuditService _auditService;
    private readonly IFeatureGateService _featureGateService;
    private readonly ILogger<TenantRoleService> _logger;

    public TenantRoleService(
        ITenantRoleRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        UserContext userContext,
        IAuditService auditService,
        IFeatureGateService featureGateService,
        ILogger<TenantRoleService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _auditService = auditService;
        _featureGateService = featureGateService;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TenantRoleDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var roles = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(roles.Select(MapToDto).ToList() as IReadOnlyList<TenantRoleDto>);
    }

    public async Task<Result<TenantRoleDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var role = await _repository.GetByIdAsync(id, cancellationToken);

        if (role is null)
            return Result.Failure<TenantRoleDto>("Role not found.", "NOT_FOUND");

        return Result.Success(MapToDto(role));
    }

    public async Task<Result<TenantRoleDto>> CreateAsync(
        CreateTenantRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();

        if (!await _featureGateService.IsEnabledAsync(FeatureKeys.CustomRoles, cancellationToken))
        {
            return Result.Failure<TenantRoleDto>(
                "Custom roles are not available on the current plan. Upgrade to Pro or Enterprise.",
                "PLAN_LIMIT_EXCEEDED");
        }

        if (await _repository.SlugExistsAsync(request.Slug, cancellationToken: cancellationToken))
            return Result.Failure<TenantRoleDto>("A role with this slug already exists.", "CONFLICT");

        var role = TenantRole.Create(
            _tenantContext.TenantId,
            request.Slug,
            request.Permissions,
            isSystem: false,
            _userContext.UserId);

        await _repository.AddAsync(role, cancellationToken);

        await _auditService.LogAsync(
            new AuditEntry("role.created", nameof(TenantRole), role.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created tenant role {RoleSlug} for tenant {TenantId}", role.Slug, _tenantContext.TenantId);

        return Result.Success(MapToDto(role));
    }

    public async Task<Result<TenantRoleDto>> UpdateAsync(
        Guid id,
        UpdateTenantRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var role = await _repository.GetByIdAsync(id, cancellationToken);

        if (role is null)
            return Result.Failure<TenantRoleDto>("Role not found.", "NOT_FOUND");

        if (role.IsSystem)
            return Result.Failure<TenantRoleDto>("System roles cannot be modified.", "CONFLICT");

        role.UpdatePermissions(request.Permissions, _userContext.UserId);
        await _repository.UpdateAsync(role, cancellationToken);

        await _auditService.LogAsync(
            new AuditEntry("role.updated", nameof(TenantRole), role.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToDto(role));
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var role = await _repository.GetByIdAsync(id, cancellationToken);

        if (role is null)
            return Result.Failure("Role not found.", "NOT_FOUND");

        if (role.IsSystem)
            return Result.Failure("System roles cannot be deleted.", "CONFLICT");

        role.Deactivate(_userContext.UserId);
        await _repository.UpdateAsync(role, cancellationToken);

        await _auditService.LogAsync(
            new AuditEntry("role.deactivated", nameof(TenantRole), role.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private void EnsureTenantResolved()
    {
        if (!_tenantContext.IsResolved)
            throw new InvalidOperationException("Tenant context is not resolved.");
    }

    private static TenantRoleDto MapToDto(TenantRole role) =>
        new(role.Id, role.Slug, role.GetPermissions(), role.IsSystem, role.IsActive);
}
