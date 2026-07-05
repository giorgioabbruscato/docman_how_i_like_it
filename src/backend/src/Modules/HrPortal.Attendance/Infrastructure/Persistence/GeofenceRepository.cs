using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Attendance.Infrastructure.Persistence;

internal sealed class GeofenceRepository : IGeofenceRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public GeofenceRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<GeofenceZone?> GetZoneByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<GeofenceZone>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(z => z.Id == id, cancellationToken);

    public async Task<IReadOnlyList<GeofenceZone>> GetAllZonesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<GeofenceZone>()
            .ApplyTenantScope(_accessor.Current)
            .OrderBy(z => z.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<GeofenceZone>> GetActiveZonesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<GeofenceZone>()
            .ApplyTenantScope(_accessor.Current)
            .Where(z => z.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<GeofenceSettings?> GetSettingsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<GeofenceSettings>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddZoneAsync(GeofenceZone zone, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<GeofenceZone>().AddAsync(zone, cancellationToken);

    public Task UpdateZoneAsync(GeofenceZone zone, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<GeofenceZone>().Update(zone);
        return Task.CompletedTask;
    }

    public Task DeleteZoneAsync(GeofenceZone zone, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<GeofenceZone>().Remove(zone);
        return Task.CompletedTask;
    }

    public async Task AddSettingsAsync(GeofenceSettings settings, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<GeofenceSettings>().AddAsync(settings, cancellationToken);

    public Task UpdateSettingsAsync(GeofenceSettings settings, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<GeofenceSettings>().Update(settings);
        return Task.CompletedTask;
    }
}
