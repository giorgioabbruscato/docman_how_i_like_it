namespace HrPortal.Tenancy.Application.Dtos;

public sealed record PlatformDashboardSummaryDto(
    int TotalTenants,
    int TotalEmployees,
    int ActiveEmployeesLast30Days,
    int TotalTimeEntriesLast30Days,
    int LicenseSeatsUsed,
    int LicenseSeatsTotal);

public sealed record PlatformTenantMetricsDto(
    Guid TenantId,
    string Slug,
    string Name,
    int EmployeeCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastActivityAt);

public sealed record PlatformTenantSummaryDto(
    Guid TenantId,
    string Slug,
    string Name,
    int EmployeeCount,
    int ActiveProjects,
    int TimeEntriesThisMonth,
    int AttendanceSessionsThisMonth,
    int LeaveRequestsPending,
    long? StorageUsedBytes);

public sealed record PlatformUsageDto(
    IReadOnlyList<PlatformUsageTrendPointDto> TenantGrowth,
    IReadOnlyList<PlatformUsageTrendPointDto> TimeEntriesByMonth);

public sealed record PlatformUsageTrendPointDto(
    string Period,
    int Count);
