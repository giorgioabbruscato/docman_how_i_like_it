using HrPortal.Tenancy.Domain;

namespace HrPortal.Api.Infrastructure.OpenApi;

public sealed record TenantListItemDto(Guid Id, string Name, string Slug, bool IsActive);

public sealed record TenantCreatedDto(Guid Id, string Name, string Slug);

public sealed record PlatformTenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    bool IsSuspended,
    string Plan,
    IReadOnlyList<string> Modules,
    TenantFeatures Features);

public sealed record UpdateTenantPlanRequest(string Plan);

public sealed record UpdateTenantFeaturesRequest(
    int? MaxEmployees = null,
    bool? CustomRoles = null,
    bool? AuditLog = null,
    bool? AdvancedReports = null);
