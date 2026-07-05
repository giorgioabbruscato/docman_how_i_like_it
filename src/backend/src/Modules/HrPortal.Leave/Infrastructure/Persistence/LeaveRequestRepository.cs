using HrPortal.Leave.Application;
using HrPortal.Leave.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Leave.Infrastructure.Persistence;

internal sealed class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public LeaveRequestRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<LeaveRequest>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<IReadOnlyList<LeaveRequest>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<LeaveRequest>()
            .ApplyTenantScope(_accessor.Current)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasOverlappingApprovedAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<LeaveRequest>()
            .ApplyTenantScope(_accessor.Current)
            .Where(l => l.EmployeeId == employeeId)
            .Where(l => l.Status == LeaveStatus.Approved)
            .Where(l => l.StartDate <= endDate && l.EndDate >= startDate);

        if (excludeId.HasValue)
            query = query.Where(l => l.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetApprovedAnnualDaysInYearAsync(
        Guid employeeId,
        int year,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var yearStart = new DateOnly(year, 1, 1);
        var yearEnd = new DateOnly(year, 12, 31);

        var query = _dbContext.Set<LeaveRequest>()
            .ApplyTenantScope(_accessor.Current)
            .Where(l => l.EmployeeId == employeeId)
            .Where(l => l.Type == LeaveType.Annual)
            .Where(l => l.Status == LeaveStatus.Approved)
            .Where(l => l.StartDate <= yearEnd && l.EndDate >= yearStart);

        if (excludeId.HasValue)
            query = query.Where(l => l.Id != excludeId.Value);

        var requests = await query.ToListAsync(cancellationToken);

        return requests.Sum(r =>
        {
            var effectiveStart = r.StartDate > yearStart ? r.StartDate : yearStart;
            var effectiveEnd = r.EndDate < yearEnd ? r.EndDate : yearEnd;
            return effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
        });
    }

    public async Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<LeaveRequest>().AddAsync(leaveRequest, cancellationToken);

    public Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<LeaveRequest>().Update(leaveRequest);
        return Task.CompletedTask;
    }
}
