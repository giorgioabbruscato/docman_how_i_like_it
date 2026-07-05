using HrPortal.AccessControl.Application;
using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;
using HrPortal.Employees.Application;
using HrPortal.Employees.Domain;
using HrPortal.Notifications;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace HrPortal.UnitTests.Notifications;

public sealed class AttendanceReminderNotificationTests
{
    [Fact]
    public async Task ProcessRemindersAsync_Notifies_WhenNoCheckInAfterThreshold()
    {
        var tenantId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var employee = Employee.Create(
            tenantId,
            "Jane",
            "Doe",
            "jane@demo.local",
            new DateOnly(2024, 1, 1));

        var employeeRepository = new Mock<IEmployeeRepository>();
        employeeRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { employee });

        var sessionRepository = new Mock<IAttendanceSessionRepository>();
        sessionRepository.Setup(r => r.GetOpenSessionAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceSession?)null);
        sessionRepository.Setup(r => r.GetByEmployeeAndDateRangeAsync(
                employee.Id,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceSession>());

        var notificationService = new Mock<INotificationService>();
        var recipientResolver = new Mock<INotificationRecipientResolver>();
        recipientResolver.Setup(r => r.ResolveForEmployeeAsync(employee.Id, employee.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRecipient(userId, userId.ToString()));

        var employeeLookup = new Mock<IEmployeeLookup>();
        var tenantContext = TenantContext.CreateTenantOnly(tenantId, "demo");
        var options = Options.Create(new AttendanceReminderOptions
        {
            CheckInReminderHour = new TimeOnly(0, 0),
            CheckOutReminderHour = new TimeOnly(23, 0)
        });

        var service = new AttendanceReminderService(
            employeeRepository.Object,
            sessionRepository.Object,
            notificationService.Object,
            recipientResolver.Object,
            employeeLookup.Object,
            tenantContext,
            options,
            NullLogger<AttendanceReminderService>.Instance);

        var localMorning = DateTime.Today.AddHours(11);
        await service.ProcessRemindersAsync(localMorning.ToUniversalTime());

        notificationService.Verify(
            n => n.NotifyForgottenCheckInAsync(userId, DateOnly.FromDateTime(localMorning), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
