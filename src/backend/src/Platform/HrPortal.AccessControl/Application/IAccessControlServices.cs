using HrPortal.AccessControl.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.AccessControl.Application;

public interface IMeService
{
    Task<Result<MeDto>> GetCurrentAsync(CancellationToken cancellationToken = default);
}

public interface ITenantRoleService
{
    Task<Result<IReadOnlyList<TenantRoleDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TenantRoleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TenantRoleDto>> CreateAsync(CreateTenantRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<TenantRoleDto>> UpdateAsync(Guid id, UpdateTenantRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ITenantMembershipService
{
    Task<Result<IReadOnlyList<TenantMembershipDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TenantMembershipDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TenantMembershipDto>> CreateAsync(CreateTenantMembershipRequest request, CancellationToken cancellationToken = default);
    Task<Result<TenantMembershipDto>> UpdateAsync(Guid id, UpdateTenantMembershipRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
