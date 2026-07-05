using HrPortal.Attendance.Domain;

namespace HrPortal.Attendance.Application;

public sealed record AttendanceSessionReadFilter(
    IReadOnlyList<Guid>? AllowedEmployeeIds,
    Guid? EmployeeId);

public interface IAttendanceSessionRepository
{
    Task<AttendanceSession?> GetOpenSessionAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<AttendanceSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AttendanceSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendanceSession session, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceSession>> GetByEmployeeAndDateRangeAsync(
        Guid employeeId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceSession>> GetByEmployeeIdsAndDateRangeAsync(
        IReadOnlyList<Guid> employeeIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AttendanceSession> Items, int Total)> GetHistoryAsync(
        AttendanceSessionReadFilter filter,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
