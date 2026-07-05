using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Attendance.Domain;

public sealed class GeofenceZone : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double RadiusMeters { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }

    private GeofenceZone() { }

    public static GeofenceZone Create(
        Guid tenantId,
        string name,
        double latitude,
        double longitude,
        double radiusMeters,
        string? description = null,
        Guid? createdBy = null)
    {
        ValidateCoordinates(latitude, longitude, radiusMeters);

        return new GeofenceZone
        {
            Name = name.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            RadiusMeters = radiusMeters,
            IsActive = true,
            Description = description,
            CreatedBy = createdBy
        }.Also(z => z.SetTenant(tenantId));
    }

    public void Update(
        string name,
        double latitude,
        double longitude,
        double radiusMeters,
        bool isActive,
        string? description,
        Guid? updatedBy)
    {
        ValidateCoordinates(latitude, longitude, radiusMeters);
        Name = name.Trim();
        Latitude = latitude;
        Longitude = longitude;
        RadiusMeters = radiusMeters;
        IsActive = isActive;
        Description = description;
        MarkUpdated(updatedBy);
    }

    private static void ValidateCoordinates(double latitude, double longitude, double radiusMeters)
    {
        if (latitude is < -90 or > 90)
            throw new DomainException("Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new DomainException("Longitude must be between -180 and 180.");
        if (radiusMeters <= 0)
            throw new DomainException("Radius must be greater than zero.");
    }
}

public sealed class GeofenceSettings : AuditableEntity
{
    public bool GeofencingEnabled { get; private set; }
    public bool AllowCheckInWithoutGps { get; private set; }

    private GeofenceSettings() { }

    public static GeofenceSettings CreateDefault(Guid tenantId) =>
        new GeofenceSettings
        {
            GeofencingEnabled = false,
            AllowCheckInWithoutGps = true
        }.Also(s => s.SetTenant(tenantId));

    public void Update(bool geofencingEnabled, bool allowCheckInWithoutGps, Guid? updatedBy)
    {
        GeofencingEnabled = geofencingEnabled;
        AllowCheckInWithoutGps = allowCheckInWithoutGps;
        MarkUpdated(updatedBy);
    }
}
