using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Attendance.Domain;

public enum AttendanceSessionStatus
{
    Open,
    Closed,
    AutoClosed
}

public sealed class AttendanceSession : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public DateTime CheckIn { get; private set; }
    public DateTime? CheckOut { get; private set; }
    public double? LatitudeCheckIn { get; private set; }
    public double? LongitudeCheckIn { get; private set; }
    public double? LatitudeCheckOut { get; private set; }
    public double? LongitudeCheckOut { get; private set; }
    public double? AccuracyCheckIn { get; private set; }
    public double? AccuracyCheckOut { get; private set; }
    public string? IPAddress { get; private set; }
    public string? Device { get; private set; }
    public string? Browser { get; private set; }
    public int? WorkedMinutes { get; private set; }
    public AttendanceSessionStatus Status { get; private set; }
    public Guid? MatchedGeofenceZoneId { get; private set; }
    public bool GpsUnavailableAtCheckIn { get; private set; }

    private AttendanceSession() { }

    public static AttendanceSession Create(
        Guid tenantId,
        Guid employeeId,
        DateTime checkIn,
        string? ipAddress = null,
        double? latitudeCheckIn = null,
        double? longitudeCheckIn = null,
        double? accuracyCheckIn = null,
        string? device = null,
        string? browser = null,
        Guid? matchedGeofenceZoneId = null,
        bool gpsUnavailableAtCheckIn = false,
        Guid? createdBy = null)
    {
        return new AttendanceSession
        {
            EmployeeId = employeeId,
            CheckIn = checkIn,
            LatitudeCheckIn = latitudeCheckIn,
            LongitudeCheckIn = longitudeCheckIn,
            AccuracyCheckIn = accuracyCheckIn,
            IPAddress = ipAddress,
            Device = device,
            Browser = browser,
            Status = AttendanceSessionStatus.Open,
            MatchedGeofenceZoneId = matchedGeofenceZoneId,
            GpsUnavailableAtCheckIn = gpsUnavailableAtCheckIn,
            CreatedBy = createdBy
        }.Also(s => s.SetTenant(tenantId));
    }

    public void Close(
        DateTime checkOut,
        double? latitudeCheckOut = null,
        double? longitudeCheckOut = null,
        double? accuracyCheckOut = null,
        Guid? updatedBy = null)
    {
        if (Status != AttendanceSessionStatus.Open)
            throw new DomainException("Session is already closed.");

        if (checkOut <= CheckIn)
            throw new DomainException("Check-out time must be after check-in time.");

        CheckOut = checkOut;
        LatitudeCheckOut = latitudeCheckOut;
        LongitudeCheckOut = longitudeCheckOut;
        AccuracyCheckOut = accuracyCheckOut;
        WorkedMinutes = CalculateWorkedMinutes(CheckIn, checkOut);
        Status = AttendanceSessionStatus.Closed;
        MarkUpdated(updatedBy);
    }

    public static int CalculateWorkedMinutes(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
            throw new DomainException("Check-out time must be after check-in time.");

        return (int)Math.Round((checkOut - checkIn).TotalMinutes);
    }
}

internal static class AttendanceSessionExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
