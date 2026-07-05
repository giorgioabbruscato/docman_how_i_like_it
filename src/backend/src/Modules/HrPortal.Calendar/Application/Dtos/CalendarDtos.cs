namespace HrPortal.Calendar.Application.Dtos;

public sealed record PublicHolidayDto(
    Guid Id,
    string Name,
    DateOnly Date,
    bool IsRecurring,
    string? CountryCode);

public sealed record CreatePublicHolidayRequest(
    string Name,
    DateOnly Date,
    bool IsRecurring = false,
    string? CountryCode = null);

public sealed record UpdatePublicHolidayRequest(
    string Name,
    DateOnly Date,
    bool IsRecurring = false,
    string? CountryCode = null);

public sealed record GetCalendarEventsQuery(
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? DepartmentId = null,
    Guid? EmployeeId = null);
