using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.Audit.Application;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Attendance.Application.Commands;

public sealed class CheckOutCommandHandler
{
    private readonly IAttendanceSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<CheckOutCommandHandler> _logger;

    public CheckOutCommandHandler(
        IAttendanceSessionRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<CheckOutCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<CheckOutResponseDto>> HandleAsync(
        CheckOutRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var contextResult = AttendanceSessionReadScope.EnsureEmployeeContext(_tenantContext);
        if (!contextResult.IsSuccess)
            return Result.Failure<CheckOutResponseDto>(contextResult.Error!, contextResult.ErrorCode);

        var employeeId = _tenantContext.EmployeeId!.Value;
        var session = await _repository.GetOpenSessionAsync(employeeId, cancellationToken);
        if (session is null)
            return Result.Failure<CheckOutResponseDto>("No open attendance session found.", "NOT_FOUND");

        try
        {
            session.Close(
                DateTime.UtcNow,
                request.Latitude,
                request.Longitude,
                request.Accuracy,
                _tenantContext.UserId);
        }
        catch (DomainException ex)
        {
            return Result.Failure<CheckOutResponseDto>(ex.Message, "VALIDATION_ERROR");
        }

        await _repository.UpdateAsync(session, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "attendance_session.check_out",
            nameof(AttendanceSession),
            session.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} checked out", employeeId);
        return Result.Success(new CheckOutResponseDto(
            session.Id,
            session.CheckIn,
            session.CheckOut!.Value,
            session.WorkedMinutes!.Value,
            session.Status.ToString()));
    }
}
