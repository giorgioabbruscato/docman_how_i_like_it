using HrPortal.Calendar.Application;
using HrPortal.Calendar.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Calendar.Infrastructure.Persistence;

internal sealed class PublicHolidayRepository : IPublicHolidayRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public PublicHolidayRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<PublicHoliday?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<PublicHoliday>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PublicHoliday>> GetInDateRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<PublicHoliday>()
            .ApplyTenantScope(_accessor.Current)
            .Where(h => h.Date >= fromDate && h.Date <= toDate)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PublicHoliday>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<PublicHoliday>()
            .ApplyTenantScope(_accessor.Current)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PublicHoliday holiday, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<PublicHoliday>().AddAsync(holiday, cancellationToken);

    public Task UpdateAsync(PublicHoliday holiday, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<PublicHoliday>().Update(holiday);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PublicHoliday holiday, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<PublicHoliday>().Remove(holiday);
        return Task.CompletedTask;
    }
}

internal sealed class SmartWorkingScheduleRepository : ISmartWorkingScheduleRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public SmartWorkingScheduleRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<SmartWorkingSchedule?> GetForTenantAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<SmartWorkingSchedule>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(SmartWorkingSchedule schedule, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<SmartWorkingSchedule>().AddAsync(schedule, cancellationToken);

    public Task UpdateAsync(SmartWorkingSchedule schedule, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<SmartWorkingSchedule>().Update(schedule);
        return Task.CompletedTask;
    }
}
