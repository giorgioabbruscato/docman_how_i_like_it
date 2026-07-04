using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Leave.Domain;

public enum LeaveType
{
    Annual,
    Sick,
    Personal,
    Maternity,
    Paternity,
    Unpaid
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public sealed class LeaveRequest : AuditableEntity
{
    public const int MaxAnnualLeaveDays = 25;

    public Guid EmployeeId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public LeaveType Type { get; private set; }
    public LeaveStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    private LeaveRequest() { }

    public static LeaveRequest Create(
        Guid tenantId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly endDate,
        LeaveType type,
        string? reason = null,
        Guid? createdBy = null)
    {
        if (endDate < startDate)
            throw new DomainException("End date must be on or after start date.");

        return new LeaveRequest
        {
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            Type = type,
            Reason = reason,
            Status = LeaveStatus.Pending,
            CreatedBy = createdBy
        }.Also(l => l.SetTenant(tenantId));
    }

    public int DayCount => EndDate.DayNumber - StartDate.DayNumber + 1;

    public void Approve(Guid approvedBy)
    {
        if (Status != LeaveStatus.Pending)
            throw new DomainException("Only pending leave requests can be approved.");

        Status = LeaveStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        MarkUpdated(approvedBy);
    }

    public void Reject(Guid rejectedBy, string? reason = null)
    {
        if (Status != LeaveStatus.Pending)
            throw new DomainException("Only pending leave requests can be rejected.");

        Status = LeaveStatus.Rejected;
        ApprovedBy = rejectedBy;
        ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
            Reason = reason;

        MarkUpdated(rejectedBy);
    }

    public void Cancel(Guid? cancelledBy)
    {
        if (Status != LeaveStatus.Pending)
            throw new DomainException("Only pending leave requests can be cancelled.");

        Status = LeaveStatus.Cancelled;
        MarkUpdated(cancelledBy);
    }
}

internal static class LeaveRequestExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
