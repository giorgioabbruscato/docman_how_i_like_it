using HrPortal.Tenancy.Application.Dtos;

namespace HrPortal.Tenancy.Application;

public interface IPlatformMetricsService
{
    Task<PlatformDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformTenantMetricsDto>> GetTenantsAsync(CancellationToken cancellationToken = default);

    Task<PlatformTenantSummaryDto?> GetTenantSummaryAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<PlatformUsageDto> GetUsageAsync(CancellationToken cancellationToken = default);
}
