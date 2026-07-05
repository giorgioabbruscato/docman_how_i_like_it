using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.TimeTracking.Infrastructure.Persistence;

internal sealed class TimesheetRepository : ITimesheetRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public TimesheetRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<TimesheetSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimesheetSubmission>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TimesheetSubmission?> GetByIdWithEntriesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimesheetSubmission>()
            .ApplyTenantScope(_accessor.Current)
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<PagedResult<TimesheetSubmission>> GetPagedAsync(
        GetTimesheetsQuery query,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var submissions = ApplyFilters(
            _dbContext.Set<TimesheetSubmission>()
                .ApplyTenantScope(_accessor.Current)
                .Include(t => t.Entries)
                .AsQueryable(),
            query,
            allowedEmployeeIds);

        var totalCount = await submissions.CountAsync(cancellationToken);

        var items = await submissions
            .OrderByDescending(t => t.PeriodStart)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TimesheetSubmission>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<bool> HasOverlappingPeriodAsync(
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<TimesheetSubmission>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.EmployeeId == employeeId
                        && t.PeriodStart <= periodEnd
                        && t.PeriodEnd >= periodStart);

        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeEntry>> GetCompletedEntriesForPeriodAsync(
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = periodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = periodEnd.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return await _dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.EmployeeId == employeeId
                        && t.EndTime != null
                        && t.StartTime >= fromUtc
                        && t.StartTime < toUtc)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlySet<Guid>> GetApprovedTimeEntryIdsAsync(
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<TimesheetSubmission>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.Status == TimesheetStatus.Approved);

        if (allowedEmployeeIds is not null)
            query = query.Where(t => allowedEmployeeIds.Contains(t.EmployeeId));

        if (employeeId.HasValue)
            query = query.Where(t => t.EmployeeId == employeeId.Value);

        if (fromDate.HasValue)
            query = query.Where(t => t.PeriodEnd >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.PeriodStart <= toDate.Value);

        var submissionIds = await query.Select(t => t.Id).ToListAsync(cancellationToken);

        if (submissionIds.Count == 0)
            return new HashSet<Guid>();

        var entryIds = await _dbContext.Set<TimesheetSubmissionEntry>()
            .ApplyTenantScope(_accessor.Current)
            .Where(e => submissionIds.Contains(e.TimesheetSubmissionId))
            .Select(e => e.TimeEntryId)
            .ToListAsync(cancellationToken);

        return entryIds.ToHashSet();
    }

    public async Task<TimesheetApproval?> GetLatestApprovalAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimesheetApproval>()
            .ApplyTenantScope(_accessor.Current)
            .Where(a => a.TimesheetSubmissionId == submissionId)
            .OrderByDescending(a => a.DecidedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(TimesheetSubmission submission, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimesheetSubmission>().AddAsync(submission, cancellationToken);

    public async Task AddApprovalAsync(TimesheetApproval approval, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TimesheetApproval>().AddAsync(approval, cancellationToken);

    public Task UpdateAsync(TimesheetSubmission submission, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TimesheetSubmission>().Update(submission);
        return Task.CompletedTask;
    }

    private static IQueryable<TimesheetSubmission> ApplyFilters(
        IQueryable<TimesheetSubmission> query,
        GetTimesheetsQuery filters,
        IReadOnlyList<Guid>? allowedEmployeeIds)
    {
        if (allowedEmployeeIds is not null)
            query = query.Where(t => allowedEmployeeIds.Contains(t.EmployeeId));

        if (filters.EmployeeId.HasValue)
            query = query.Where(t => t.EmployeeId == filters.EmployeeId.Value);

        if (!string.IsNullOrWhiteSpace(filters.Status)
            && Enum.TryParse<TimesheetStatus>(filters.Status, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (filters.FromDate.HasValue)
            query = query.Where(t => t.PeriodEnd >= filters.FromDate.Value);

        if (filters.ToDate.HasValue)
            query = query.Where(t => t.PeriodStart <= filters.ToDate.Value);

        return query;
    }
}
