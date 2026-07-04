using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Attendance.Infrastructure.Persistence;

internal sealed class AttendanceRepository : IAttendanceRepository
{
    private readonly DbContext _dbContext;

    public AttendanceRepository(DbContext dbContext) => _dbContext = dbContext;

    public async Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceRecord>().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<AttendanceRecord?> GetByEmployeeAndDateAsync(
        Guid employeeId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceRecord>()
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date, cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceRecord>()
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> GetByDateRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceRecord>()
            .Where(a => a.Date >= from && a.Date <= to)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AttendanceRecord record, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceRecord>().AddAsync(record, cancellationToken);

    public Task UpdateAsync(AttendanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<AttendanceRecord>().Update(record);
        return Task.CompletedTask;
    }
}
