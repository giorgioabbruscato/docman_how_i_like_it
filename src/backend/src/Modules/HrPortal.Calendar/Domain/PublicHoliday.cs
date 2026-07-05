using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Calendar.Domain;

public sealed class PublicHoliday : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public bool IsRecurring { get; private set; }
    public string? CountryCode { get; private set; }

    private PublicHoliday() { }

    public static PublicHoliday Create(
        Guid tenantId,
        string name,
        DateOnly date,
        bool isRecurring = false,
        string? countryCode = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Holiday name is required.");

        return new PublicHoliday
        {
            Name = name.Trim(),
            Date = date,
            IsRecurring = isRecurring,
            CountryCode = countryCode,
            CreatedBy = createdBy
        }.Also(h => h.SetTenant(tenantId));
    }

    public void Update(string name, DateOnly date, bool isRecurring, string? countryCode, Guid? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Holiday name is required.");

        Name = name.Trim();
        Date = date;
        IsRecurring = isRecurring;
        CountryCode = countryCode;
        MarkUpdated(updatedBy);
    }
}

public sealed class SmartWorkingSchedule : AuditableEntity
{
    public string WeekdaysJson { get; private set; } = "[]";

    private SmartWorkingSchedule() { }

    public static SmartWorkingSchedule Create(Guid tenantId, IReadOnlyList<int> weekdays, Guid? createdBy = null)
    {
        var schedule = new SmartWorkingSchedule { CreatedBy = createdBy };
        schedule.SetTenant(tenantId);
        schedule.SetWeekdays(weekdays);
        return schedule;
    }

    public IReadOnlyList<int> GetWeekdays() =>
        System.Text.Json.JsonSerializer.Deserialize<List<int>>(WeekdaysJson) ?? [];

    public void SetWeekdays(IReadOnlyList<int> weekdays) =>
        WeekdaysJson = System.Text.Json.JsonSerializer.Serialize(weekdays.Distinct().OrderBy(d => d).ToList());

    public void UpdateWeekdays(IReadOnlyList<int> weekdays, Guid? updatedBy)
    {
        SetWeekdays(weekdays);
        MarkUpdated(updatedBy);
    }
}

internal static class CalendarEntityExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
