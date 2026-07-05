using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Attendance.Application;

public interface IGeofenceService
{
    Task<Result<IReadOnlyList<GeofenceZoneDto>>> GetZonesAsync(CancellationToken cancellationToken = default);
    Task<Result<GeofenceZoneDto>> CreateZoneAsync(CreateGeofenceZoneRequest request, CancellationToken cancellationToken = default);
    Task<Result<GeofenceZoneDto>> UpdateZoneAsync(Guid id, UpdateGeofenceZoneRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteZoneAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<GeofenceSettingsDto>> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<Result<GeofenceSettingsDto>> UpdateSettingsAsync(UpdateGeofenceSettingsRequest request, CancellationToken cancellationToken = default);
}

internal sealed class GeofenceService : IGeofenceService
{
    private readonly IGeofenceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;

    public GeofenceService(IGeofenceRepository repository, IUnitOfWork unitOfWork, TenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<GeofenceZoneDto>>> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        var zones = await _repository.GetAllZonesAsync(cancellationToken);
        return Result.Success<IReadOnlyList<GeofenceZoneDto>>(zones.Select(MapZone).ToList());
    }

    public async Task<Result<GeofenceZoneDto>> CreateZoneAsync(
        CreateGeofenceZoneRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var zone = GeofenceZone.Create(
                _tenantContext.TenantId,
                request.Name,
                request.Latitude,
                request.Longitude,
                request.RadiusMeters,
                request.Description,
                _tenantContext.UserId);

            await _repository.AddZoneAsync(zone, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(MapZone(zone));
        }
        catch (DomainException ex)
        {
            return Result.Failure<GeofenceZoneDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }

    public async Task<Result<GeofenceZoneDto>> UpdateZoneAsync(
        Guid id,
        UpdateGeofenceZoneRequest request,
        CancellationToken cancellationToken = default)
    {
        var zone = await _repository.GetZoneByIdAsync(id, cancellationToken);
        if (zone is null)
            return Result.Failure<GeofenceZoneDto>("Geofence zone not found.", "NOT_FOUND");

        try
        {
            zone.Update(request.Name, request.Latitude, request.Longitude, request.RadiusMeters,
                request.IsActive, request.Description, _tenantContext.UserId);
            await _repository.UpdateZoneAsync(zone, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(MapZone(zone));
        }
        catch (DomainException ex)
        {
            return Result.Failure<GeofenceZoneDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }

    public async Task<Result> DeleteZoneAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var zone = await _repository.GetZoneByIdAsync(id, cancellationToken);
        if (zone is null)
            return Result.Failure("Geofence zone not found.", "NOT_FOUND");

        await _repository.DeleteZoneAsync(zone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<GeofenceSettingsDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetSettingsAsync(cancellationToken);
        if (settings is null)
            return Result.Success(new GeofenceSettingsDto(false, true));

        return Result.Success(new GeofenceSettingsDto(settings.GeofencingEnabled, settings.AllowCheckInWithoutGps));
    }

    public async Task<Result<GeofenceSettingsDto>> UpdateSettingsAsync(
        UpdateGeofenceSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetSettingsAsync(cancellationToken);
        if (settings is null)
        {
            settings = GeofenceSettings.CreateDefault(_tenantContext.TenantId);
            settings.Update(request.GeofencingEnabled, request.AllowCheckInWithoutGps, _tenantContext.UserId);
            await _repository.AddSettingsAsync(settings, cancellationToken);
        }
        else
        {
            settings.Update(request.GeofencingEnabled, request.AllowCheckInWithoutGps, _tenantContext.UserId);
            await _repository.UpdateSettingsAsync(settings, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new GeofenceSettingsDto(settings.GeofencingEnabled, settings.AllowCheckInWithoutGps));
    }

    private static GeofenceZoneDto MapZone(GeofenceZone zone) =>
        new(zone.Id, zone.Name, zone.Latitude, zone.Longitude, zone.RadiusMeters, zone.IsActive, zone.Description);
}
