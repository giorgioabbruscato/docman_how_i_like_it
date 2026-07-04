using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Attendance.Domain;

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    HalfDay,
    Remote
}

public sealed class AttendanceRecord : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly? CheckIn { get; private set; }
    public TimeOnly? CheckOut { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private AttendanceRecord() { }

    public static AttendanceRecord Create(
        Guid tenantId,
        Guid employeeId,
        DateOnly date,
        Guid? createdBy = null)
    {
        return new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = date,
            Status = AttendanceStatus.Present,
            CreatedBy = createdBy
        }.Also(a => a.SetTenant(tenantId));
    }

    public void RecordCheckIn(TimeOnly time)
    {
        if (CheckIn.HasValue)
            throw new DomainException("Employee has already checked in for this date.");

        CheckIn = time;
        Status = AttendanceStatus.Present;
    }

    public void RecordCheckOut(TimeOnly time)
    {
        if (!CheckIn.HasValue)
            throw new DomainException("Cannot check out without checking in first.");

        if (CheckOut.HasValue)
            throw new DomainException("Employee has already checked out for this date.");

        if (time <= CheckIn.Value)
            throw new DomainException("Check-out time must be after check-in time.");

        CheckOut = time;
    }

    public void UpdateNotes(string? notes, Guid? updatedBy)
    {
        Notes = notes;
        MarkUpdated(updatedBy);
    }
}

internal static class AttendanceRecordExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
