using HrPortal.Attendance.Domain;

namespace HrPortal.Attendance.Application;

public interface IGeofenceRepository
{
    Task<GeofenceZone?> GetZoneByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeofenceZone>> GetAllZonesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeofenceZone>> GetActiveZonesAsync(CancellationToken cancellationToken = default);
    Task<GeofenceSettings?> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task AddZoneAsync(GeofenceZone zone, CancellationToken cancellationToken = default);
    Task UpdateZoneAsync(GeofenceZone zone, CancellationToken cancellationToken = default);
    Task DeleteZoneAsync(GeofenceZone zone, CancellationToken cancellationToken = default);
    Task AddSettingsAsync(GeofenceSettings settings, CancellationToken cancellationToken = default);
    Task UpdateSettingsAsync(GeofenceSettings settings, CancellationToken cancellationToken = default);
}
