using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Domain;
using HrPortal.Audit.Application;
using HrPortal.Identity;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class TenantMembershipService : ITenantMembershipService
{
    private readonly ITenantMembershipRepository _repository;
    private readonly ITenantRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly UserContext _userContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantMembershipService> _logger;

    public TenantMembershipService(
        ITenantMembershipRepository repository,
        ITenantRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        UserContext userContext,
        IAuditService auditService,
        ILogger<TenantMembershipService> logger)
    {
        _repository = repository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TenantMembershipDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var memberships = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(memberships.Select(MapToDto).ToList() as IReadOnlyList<TenantMembershipDto>);
    }

    public async Task<Result<TenantMembershipDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var membership = await _repository.GetByIdAsync(id, cancellationToken);

        if (membership is null)
            return Result.Failure<TenantMembershipDto>("Membership not found.", "NOT_FOUND");

        return Result.Success(MapToDto(membership));
    }

    public async Task<Result<TenantMembershipDto>> CreateAsync(
        CreateTenantMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();

        if (await _repository.ActiveMembershipExistsAsync(request.UserId, cancellationToken: cancellationToken))
            return Result.Failure<TenantMembershipDto>("An active membership already exists for this user.", "CONFLICT");

        var roleValidation = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
        if (!roleValidation.IsSuccess)
            return Result.Failure<TenantMembershipDto>(roleValidation.Error!, roleValidation.ErrorCode);

        var membership = TenantMembership.Create(
            _tenantContext.TenantId,
            request.UserId,
            request.RoleIds,
            request.EmployeeId,
            request.Attributes,
            _userContext.UserId);

        await _repository.AddAsync(membership, cancellationToken);

        await _auditService.LogAsync(
            new AuditEntry("membership.created", nameof(TenantMembership), membership.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Created membership for user {UserId} in tenant {TenantId}",
            request.UserId,
            _tenantContext.TenantId);

        return Result.Success(MapToDto(membership));
    }

    public async Task<Result<TenantMembershipDto>> UpdateAsync(
        Guid id,
        UpdateTenantMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var membership = await _repository.GetByIdAsync(id, cancellationToken);

        if (membership is null)
            return Result.Failure<TenantMembershipDto>("Membership not found.", "NOT_FOUND");

        var roleValidation = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
        if (!roleValidation.IsSuccess)
            return Result.Failure<TenantMembershipDto>(roleValidation.Error!, roleValidation.ErrorCode);

        membership.UpdateRoles(
            request.RoleIds,
            request.EmployeeId,
            request.Attributes,
            _userContext.UserId);

        await _repository.UpdateAsync(membership, cancellationToken);

        await _auditService.LogAsync(
            new AuditEntry("membership.updated", nameof(TenantMembership), membership.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToDto(membership));
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();
        var membership = await _repository.GetByIdAsync(id, cancellationToken);

        if (membership is null)
            return Result.Failure("Membership not found.", "NOT_FOUND");

        membership.Deactivate(_userContext.UserId);
        await _repository.UpdateAsync(membership, cancellationToken);

        await _auditService.LogAsync(
            new AuditEntry("membership.deactivated", nameof(TenantMembership), membership.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> ValidateRoleIdsAsync(
        IReadOnlyList<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
            return Result.Failure("At least one role is required.", "VALIDATION_ERROR");

        var roles = await _roleRepository.GetByIdsAsync(roleIds, cancellationToken);

        if (roles.Count != roleIds.Distinct().Count())
            return Result.Failure("One or more role IDs are invalid or inactive.", "VALIDATION_ERROR");

        return Result.Success();
    }

    private void EnsureTenantResolved()
    {
        if (!_tenantContext.IsResolved)
            throw new InvalidOperationException("Tenant context is not resolved.");
    }

    private static TenantMembershipDto MapToDto(TenantMembership membership) =>
        new(
            membership.Id,
            membership.UserId,
            membership.GetRoleIds(),
            membership.EmployeeId,
            membership.GetAttributes(),
            membership.IsActive);
}
