using HrPortal.AccessControl.Application;
using HrPortal.Attendance.Application;
using HrPortal.Employees.Application;
using HrPortal.Employees.Domain;
using HrPortal.Notifications;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace HrPortal.Attendance.Application;

public sealed class AttendanceReminderOptions
{
    public const string SectionName = "AttendanceReminders";

    public TimeOnly CheckInReminderHour { get; set; } = new(10, 0);
    public TimeOnly CheckOutReminderHour { get; set; } = new(18, 0);
}

public interface IAttendanceReminderService
{
    Task ProcessRemindersAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}

internal sealed class AttendanceReminderService : IAttendanceReminderService
{
    private static readonly ConcurrentDictionary<string, byte> SentReminders = new();

    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceSessionRepository _sessionRepository;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly TenantContext _tenantContext;
    private readonly AttendanceReminderOptions _options;
    private readonly ILogger<AttendanceReminderService> _logger;

    public AttendanceReminderService(
        IEmployeeRepository employeeRepository,
        IAttendanceSessionRepository sessionRepository,
        INotificationService notificationService,
        INotificationRecipientResolver recipientResolver,
        IEmployeeLookup employeeLookup,
        TenantContext tenantContext,
        IOptions<AttendanceReminderOptions> options,
        ILogger<AttendanceReminderService> logger)
    {
        _employeeRepository = employeeRepository;
        _sessionRepository = sessionRepository;
        _notificationService = notificationService;
        _recipientResolver = recipientResolver;
        _employeeLookup = employeeLookup;
        _tenantContext = tenantContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ProcessRemindersAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var localNow = utcNow.ToLocalTime();
        var today = DateOnly.FromDateTime(localNow);
        var employees = (await _employeeRepository.GetAllAsync(cancellationToken))
            .Where(e => e.IsActive)
            .ToList();

        if (localNow.TimeOfDay >= _options.CheckInReminderHour.ToTimeSpan())
            await ProcessForgottenCheckInsAsync(employees, today, cancellationToken);

        if (localNow.TimeOfDay >= _options.CheckOutReminderHour.ToTimeSpan())
            await ProcessForgottenCheckOutsAsync(employees, today, cancellationToken);
    }

    private async Task ProcessForgottenCheckInsAsync(
        IReadOnlyList<Employee> employees,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        foreach (var employee in employees)
        {
            var cacheKey = BuildCacheKey("check-in", employee.Id, today);
            if (SentReminders.ContainsKey(cacheKey))
                continue;

            var openSession = await _sessionRepository.GetOpenSessionAsync(employee.Id, cancellationToken);
            if (openSession is not null)
            {
                MarkSent(cacheKey);
                continue;
            }

            var dayStart = today.ToDateTime(TimeOnly.MinValue);
            var dayEnd = today.ToDateTime(TimeOnly.MaxValue);
            var sessions = await _sessionRepository.GetByEmployeeAndDateRangeAsync(
                employee.Id,
                dayStart,
                dayEnd,
                cancellationToken);

            if (sessions.Count > 0)
            {
                MarkSent(cacheKey);
                continue;
            }

            await TryNotifyEmployeeAsync(
                employee,
                today,
                (userId, date, ct) => _notificationService.NotifyForgottenCheckInAsync(userId, date, ct),
                cancellationToken);

            MarkSent(cacheKey);
        }
    }

    private async Task ProcessForgottenCheckOutsAsync(
        IReadOnlyList<Employee> employees,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        foreach (var employee in employees)
        {
            var openSession = await _sessionRepository.GetOpenSessionAsync(employee.Id, cancellationToken);
            if (openSession is null)
                continue;

            if (DateOnly.FromDateTime(openSession.CheckIn) != today)
                continue;

            var cacheKey = BuildCacheKey("check-out", employee.Id, today);
            if (SentReminders.ContainsKey(cacheKey))
                continue;

            await TryNotifyEmployeeAsync(
                employee,
                today,
                (userId, date, ct) => _notificationService.NotifyForgottenCheckOutAsync(userId, date, ct),
                cancellationToken);

            MarkSent(cacheKey);
        }
    }

    private async Task TryNotifyEmployeeAsync(
        Employee employee,
        DateOnly date,
        Func<Guid, DateOnly, CancellationToken, Task> notify,
        CancellationToken cancellationToken)
    {
        var email = employee.Email;
        var recipient = await _recipientResolver.ResolveForEmployeeAsync(employee.Id, email, cancellationToken);

        if (!recipient.UserId.HasValue)
        {
            _logger.LogInformation(
                "Skipping attendance reminder for employee {EmployeeId}: no active membership (fallback={Fallback})",
                employee.Id,
                recipient.LogIdentifier);
            return;
        }

        await NotificationHelper.TryNotifyAsync(
            _logger,
            ct => notify(recipient.UserId.Value, date, ct),
            cancellationToken);
    }

    private string BuildCacheKey(string reminderType, Guid employeeId, DateOnly date) =>
        $"{_tenantContext.TenantId}:{employeeId}:{date:yyyy-MM-dd}:{reminderType}";

    private static void MarkSent(string cacheKey) => SentReminders.TryAdd(cacheKey, 0);
}
