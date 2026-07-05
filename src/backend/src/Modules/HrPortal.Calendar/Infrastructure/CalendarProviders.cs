using HrPortal.Calendar.Application;
using HrPortal.Calendar.Application.Dtos;
using HrPortal.Calendar.Domain;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Calendar.Infrastructure;

internal sealed class SmartWorkingCalendarProvider : ISmartWorkingCalendarProvider
{
    private readonly ISmartWorkingScheduleRepository _scheduleRepository;
    private readonly IEmployeeLookup _employeeLookup;

    public SmartWorkingCalendarProvider(
        ISmartWorkingScheduleRepository scheduleRepository,
        IEmployeeLookup employeeLookup)
    {
        _scheduleRepository = scheduleRepository;
        _employeeLookup = employeeLookup;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetEventsInDateRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<Guid>? employeeIds,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetForTenantAsync(cancellationToken);
        if (schedule is null)
            return [];

        var weekdays = schedule.GetWeekdays();
        if (weekdays.Count == 0)
            return [];

        var events = new List<CalendarEventDto>();
        var ids = employeeIds;

        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            var dow = (int)date.DayOfWeek;
            if (!weekdays.Contains(dow))
                continue;

            if (ids is null)
            {
                events.Add(new CalendarEventDto(
                    $"smart:tenant:{date:yyyy-MM-dd}",
                    "Smart working",
                    date,
                    date,
                    CalendarEventType.SmartWorking,
                    null,
                    null,
                    "#8b5cf6"));
                continue;
            }

            foreach (var employeeId in ids)
            {
                var name = await _employeeLookup.GetFullNameAsync(employeeId, cancellationToken);
                events.Add(new CalendarEventDto(
                    $"smart:{employeeId}:{date:yyyy-MM-dd}",
                    "Smart working",
                    date,
                    date,
                    CalendarEventType.SmartWorking,
                    employeeId,
                    name,
                    "#8b5cf6"));
            }
        }

        return events;
    }
}

internal sealed class PublicHolidayService : IPublicHolidayService
{
    private readonly IPublicHolidayRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;

    public PublicHolidayService(
        IPublicHolidayRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<PublicHolidayDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var holidays = await _repository.GetAllAsync(cancellationToken);
        return Result.Success<IReadOnlyList<PublicHolidayDto>>(holidays.Select(Map).ToList());
    }

    public async Task<Result<PublicHolidayDto>> CreateAsync(
        CreatePublicHolidayRequest request,
        CancellationToken cancellationToken = default)
    {
        var holiday = PublicHoliday.Create(
            _tenantContext.TenantId,
            request.Name,
            request.Date,
            request.IsRecurring,
            request.CountryCode,
            _tenantContext.UserId);

        await _repository.AddAsync(holiday, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(holiday));
    }

    public async Task<Result<PublicHolidayDto>> UpdateAsync(
        Guid id,
        UpdatePublicHolidayRequest request,
        CancellationToken cancellationToken = default)
    {
        var holiday = await _repository.GetByIdAsync(id, cancellationToken);
        if (holiday is null)
            return Result.Failure<PublicHolidayDto>("Holiday not found.", "NOT_FOUND");

        holiday.Update(request.Name, request.Date, request.IsRecurring, request.CountryCode, _tenantContext.UserId);
        await _repository.UpdateAsync(holiday, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(holiday));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var holiday = await _repository.GetByIdAsync(id, cancellationToken);
        if (holiday is null)
            return Result.Failure("Holiday not found.", "NOT_FOUND");

        await _repository.DeleteAsync(holiday, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static PublicHolidayDto Map(PublicHoliday holiday) =>
        new(holiday.Id, holiday.Name, holiday.Date, holiday.IsRecurring, holiday.CountryCode);
}
