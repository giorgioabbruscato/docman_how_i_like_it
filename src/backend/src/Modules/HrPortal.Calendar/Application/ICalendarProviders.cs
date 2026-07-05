using HrPortal.Calendar.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Calendar.Application;

public enum CalendarEventType
{
    Leave,
    Permission,
    Holiday,
    SmartWorking
}

public sealed record CalendarEventDto(
    string Id,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    CalendarEventType Type,
    Guid? EmployeeId,
    string? EmployeeName,
    string Color);

public interface ILeaveCalendarProvider
{
    Task<IReadOnlyList<CalendarEventDto>> GetApprovedInDateRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<Guid>? employeeIds,
        CancellationToken cancellationToken = default);
}

public interface ISmartWorkingCalendarProvider
{
    Task<IReadOnlyList<CalendarEventDto>> GetEventsInDateRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<Guid>? employeeIds,
        CancellationToken cancellationToken = default);
}

public interface IPublicHolidayRepository
{
    Task<Domain.PublicHoliday?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PublicHoliday>> GetInDateRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PublicHoliday>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.PublicHoliday holiday, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.PublicHoliday holiday, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.PublicHoliday holiday, CancellationToken cancellationToken = default);
}

public interface ISmartWorkingScheduleRepository
{
    Task<Domain.SmartWorkingSchedule?> GetForTenantAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.SmartWorkingSchedule schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.SmartWorkingSchedule schedule, CancellationToken cancellationToken = default);
}

public interface IPublicHolidayService
{
    Task<Result<IReadOnlyList<PublicHolidayDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PublicHolidayDto>> CreateAsync(CreatePublicHolidayRequest request, CancellationToken cancellationToken = default);
    Task<Result<PublicHolidayDto>> UpdateAsync(Guid id, UpdatePublicHolidayRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
