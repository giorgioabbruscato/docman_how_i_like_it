using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Attendance.Application.Commands;

public sealed class CheckInCommandHandler
{
    private readonly IAttendanceSessionRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<CheckInCommandHandler> _logger;

    public CheckInCommandHandler(
        IAttendanceSessionRepository repository,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<CheckInCommandHandler> logger)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<AttendanceSessionDto>> HandleAsync(
        CheckInRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var contextResult = AttendanceSessionReadScope.EnsureEmployeeContext(_tenantContext);
        if (!contextResult.IsSuccess)
            return Result.Failure<AttendanceSessionDto>(contextResult.Error!, contextResult.ErrorCode);

        var employeeId = _tenantContext.EmployeeId!.Value;

        if (!await _employeeLookup.ExistsAndIsActiveAsync(employeeId, cancellationToken))
            return Result.Failure<AttendanceSessionDto>("Employee not found or inactive.", "NOT_FOUND");

        if (await _repository.GetOpenSessionAsync(employeeId, cancellationToken) is not null)
            return Result.Failure<AttendanceSessionDto>("An open attendance session already exists.", "CONFLICT");

        if (!request.Latitude.HasValue || !request.Longitude.HasValue)
        {
            _logger.LogWarning(
                "Check-in for employee {EmployeeId} missing GPS coordinates (timezone: {Timezone})",
                employeeId,
                request.Timezone ?? "unknown");
        }

        var session = AttendanceSession.Create(
            _tenantContext.TenantId,
            employeeId,
            DateTime.UtcNow,
            ipAddress,
            request.Latitude,
            request.Longitude,
            request.Accuracy,
            request.Device,
            request.Browser,
            _tenantContext.UserId);

        await _repository.AddAsync(session, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "attendance_session.check_in",
            nameof(AttendanceSession),
            session.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} checked in", employeeId);
        return Result.Success(AttendanceSessionMapping.ToDto(session));
    }
}
