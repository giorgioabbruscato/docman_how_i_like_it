using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.TimeTracking.Infrastructure.Persistence;

internal sealed class TimeEntryRepository : ITimeEntryRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public TimeEntryRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<PagedResult<TimeEntry>> GetPagedAsync(
        GetTimeEntriesQuery query,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var entries = ApplyFilters(_dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .AsQueryable(), query, allowedEmployeeIds);

        var totalCount = await entries.CountAsync(cancellationToken);

        var items = await entries
            .OrderByDescending(t => t.StartTime)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TimeEntry>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<TimeEntry>> GetForExportAsync(
        ExportTimeEntriesQuery query,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var entries = ApplyExportFilters(
            _dbContext.Set<TimeEntry>()
                .ApplyTenantScope(_accessor.Current)
                .AsQueryable(),
            query,
            allowedEmployeeIds);

        return await entries
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<TimeEntry?> GetActiveTimerAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(t => t.EmployeeId == employeeId && t.EndTime == null, cancellationToken);

    public async Task<bool> HasOverlappingEntryAsync(
        Guid employeeId,
        DateTime start,
        DateTime? end,
        Guid? excludeEntryId = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveEnd = end ?? DateTime.UtcNow;

        var query = _dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.EmployeeId == employeeId);

        if (excludeEntryId.HasValue)
            query = query.Where(t => t.Id != excludeEntryId.Value);

        return await query.AnyAsync(
            t => t.StartTime < effectiveEnd && (t.EndTime == null || t.EndTime > start),
            cancellationToken);
    }

    public async Task<int> GetTotalMinutesForDateAsync(
        Guid employeeId,
        DateOnly date,
        Guid? excludeEntryId = null,
        CancellationToken cancellationToken = default)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var query = _dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.EmployeeId == employeeId
                        && t.StartTime >= dayStart
                        && t.StartTime < dayEnd);

        if (excludeEntryId.HasValue)
            query = query.Where(t => t.Id != excludeEntryId.Value);

        return await query.SumAsync(t => t.WorkedMinutes, cancellationToken);
    }

    public async Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimeEntry>().AddAsync(entry, cancellationToken);

    public Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TimeEntry>().Update(entry);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TimeEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TimeEntry>().Remove(entry);
        return Task.CompletedTask;
    }

    private static IQueryable<TimeEntry> ApplyFilters(
        IQueryable<TimeEntry> query,
        GetTimeEntriesQuery filters,
        IReadOnlyList<Guid>? allowedEmployeeIds)
    {
        if (allowedEmployeeIds is not null)
            query = query.Where(t => allowedEmployeeIds.Contains(t.EmployeeId));

        if (filters.EmployeeId.HasValue)
            query = query.Where(t => t.EmployeeId == filters.EmployeeId.Value);

        if (filters.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == filters.ProjectId.Value);

        if (filters.TaskId.HasValue)
            query = query.Where(t => t.TaskId == filters.TaskId.Value);

        if (filters.FromDate.HasValue)
        {
            var from = filters.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(t => t.StartTime >= from);
        }

        if (filters.ToDate.HasValue)
        {
            var to = filters.ToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(t => t.StartTime < to);
        }

        if (filters.Billable.HasValue)
            query = query.Where(t => t.Billable == filters.Billable.Value);

        return query;
    }

    private static IQueryable<TimeEntry> ApplyExportFilters(
        IQueryable<TimeEntry> query,
        ExportTimeEntriesQuery filters,
        IReadOnlyList<Guid>? allowedEmployeeIds)
    {
        if (allowedEmployeeIds is not null)
            query = query.Where(t => allowedEmployeeIds.Contains(t.EmployeeId));

        if (filters.EmployeeId.HasValue)
            query = query.Where(t => t.EmployeeId == filters.EmployeeId.Value);

        if (filters.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == filters.ProjectId.Value);

        if (filters.Month.HasValue && filters.Year.HasValue)
        {
            var from = new DateOnly(filters.Year.Value, filters.Month.Value, 1);
            var to = from.AddMonths(1);
            query = query.Where(t => t.StartTime >= from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                                     && t.StartTime < to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }
        else
        {
            if (filters.FromDate.HasValue)
            {
                var from = filters.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(t => t.StartTime >= from);
            }

            if (filters.ToDate.HasValue)
            {
                var to = filters.ToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(t => t.StartTime < to);
            }
        }

        return query.Where(t => t.EndTime != null);
    }
}
