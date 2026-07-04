using HrPortal.Attendance.Domain;

namespace HrPortal.Attendance.Application;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetByEmployeeAndDateAsync(
        Guid employeeId,
        DateOnly date,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceRecord>> GetByDateRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
    Task AddAsync(AttendanceRecord record, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendanceRecord record, CancellationToken cancellationToken = default);
}
