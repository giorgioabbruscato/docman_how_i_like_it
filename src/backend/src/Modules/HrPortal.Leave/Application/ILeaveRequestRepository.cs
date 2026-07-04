using HrPortal.Leave.Domain;

namespace HrPortal.Leave.Application;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingApprovedAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
    Task<int> GetApprovedAnnualDaysInYearAsync(
        Guid employeeId,
        int year,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
}
