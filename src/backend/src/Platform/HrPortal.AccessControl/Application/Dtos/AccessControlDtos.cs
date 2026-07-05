using HrPortal.Tenancy.Domain;

namespace HrPortal.AccessControl.Application.Dtos;

public sealed record MeDto(
    Guid UserId,
    string Email,
    Guid TenantId,
    string TenantSlug,
    Guid? EmployeeId,
    IReadOnlyList<string> RoleSlugs,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Features,
    bool IsPlatformAdmin,
    TenantFeatures PlanFeatures);

public sealed record TenantRoleDto(
    Guid Id,
    string Slug,
    IReadOnlyList<string> Permissions,
    bool IsSystem,
    bool IsActive);

public sealed record CreateTenantRoleRequest(
    string Slug,
    IReadOnlyList<string> Permissions);

public sealed record UpdateTenantRoleRequest(
    IReadOnlyList<string> Permissions);

public sealed record TenantMembershipDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<Guid> RoleIds,
    Guid? EmployeeId,
    IReadOnlyDictionary<string, string> Attributes,
    bool IsActive);

public sealed record CreateTenantMembershipRequest(
    Guid UserId,
    IReadOnlyList<Guid> RoleIds,
    Guid? EmployeeId = null,
    IReadOnlyDictionary<string, string>? Attributes = null);

public sealed record UpdateTenantMembershipRequest(
    IReadOnlyList<Guid> RoleIds,
    Guid? EmployeeId = null,
    IReadOnlyDictionary<string, string>? Attributes = null);
