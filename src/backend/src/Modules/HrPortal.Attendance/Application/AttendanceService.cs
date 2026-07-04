using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Identity;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Attendance.Application;

public interface IAttendanceService
{
    Task<Result<IReadOnlyList<AttendanceRecordDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AttendanceRecordDto>> CheckInAsync(CheckInRequest request, CancellationToken cancellationToken = default);
    Task<Result<AttendanceRecordDto>> CheckOutAsync(CheckOutRequest request, CancellationToken cancellationToken = default);
    Task<Result<AttendanceReportDto>> GetReportAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

internal sealed class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly UserContext _userContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository repository,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        UserContext userContext,
        IAuditService auditService,
        ILogger<AttendanceService> logger)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<AttendanceRecordDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(records.Select(MapToDto).ToList() as IReadOnlyList<AttendanceRecordDto>);
    }

    public async Task<Result<AttendanceRecordDto>> CheckInAsync(
        CheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();

        if (!await _employeeLookup.ExistsAndIsActiveAsync(request.EmployeeId, cancellationToken))
            return Result.Failure<AttendanceRecordDto>("Employee not found or inactive.", "NOT_FOUND");

        var date = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var time = request.Time ?? TimeOnly.FromDateTime(DateTime.UtcNow);

        var record = await _repository.GetByEmployeeAndDateAsync(request.EmployeeId, date, cancellationToken);

        if (record is null)
        {
            record = AttendanceRecord.Create(
                _tenantContext.TenantId,
                request.EmployeeId,
                date,
                _userContext.UserId);

            try
            {
                record.RecordCheckIn(time);
            }
            catch (DomainException ex)
            {
                return Result.Failure<AttendanceRecordDto>(ex.Message, "VALIDATION_ERROR");
            }

            await _repository.AddAsync(record, cancellationToken);
            await LogAndSaveAsync("attendance.check_in", record, cancellationToken);
            _logger.LogInformation("Employee {EmployeeId} checked in on {Date}", request.EmployeeId, date);
            return Result.Success(MapToDto(record));
        }

        try
        {
            record.RecordCheckIn(time);
        }
        catch (DomainException ex)
        {
            return Result.Failure<AttendanceRecordDto>(ex.Message, "VALIDATION_ERROR");
        }

        await _repository.UpdateAsync(record, cancellationToken);
        await LogAndSaveAsync("attendance.check_in", record, cancellationToken);
        _logger.LogInformation("Employee {EmployeeId} checked in on {Date}", request.EmployeeId, date);
        return Result.Success(MapToDto(record));
    }

    public async Task<Result<AttendanceRecordDto>> CheckOutAsync(
        CheckOutRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();

        if (!await _employeeLookup.ExistsAndIsActiveAsync(request.EmployeeId, cancellationToken))
            return Result.Failure<AttendanceRecordDto>("Employee not found or inactive.", "NOT_FOUND");

        var date = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var time = request.Time ?? TimeOnly.FromDateTime(DateTime.UtcNow);

        var record = await _repository.GetByEmployeeAndDateAsync(request.EmployeeId, date, cancellationToken);
        if (record is null)
            return Result.Failure<AttendanceRecordDto>("No attendance record found for this date.", "NOT_FOUND");

        try
        {
            record.RecordCheckOut(time);
        }
        catch (DomainException ex)
        {
            return Result.Failure<AttendanceRecordDto>(ex.Message, "VALIDATION_ERROR");
        }

        await _repository.UpdateAsync(record, cancellationToken);
        await LogAndSaveAsync("attendance.check_out", record, cancellationToken);
        _logger.LogInformation("Employee {EmployeeId} checked out on {Date}", request.EmployeeId, date);
        return Result.Success(MapToDto(record));
    }

    public async Task<Result<AttendanceReportDto>> GetReportAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
            return Result.Failure<AttendanceReportDto>("End date must be on or after start date.", "VALIDATION_ERROR");

        var records = await _repository.GetByDateRangeAsync(from, to, cancellationToken);

        var report = new AttendanceReportDto(
            from,
            to,
            records.Count,
            records.Count(r => r.Status == AttendanceStatus.Present),
            records.Count(r => r.Status == AttendanceStatus.Absent),
            records.Count(r => r.Status == AttendanceStatus.Late),
            records.Count(r => r.Status == AttendanceStatus.HalfDay),
            records.Count(r => r.Status == AttendanceStatus.Remote));

        return Result.Success(report);
    }

    private async Task LogAndSaveAsync(string action, AttendanceRecord record, CancellationToken cancellationToken)
    {
        await _auditService.LogAsync(new AuditEntry(action, nameof(AttendanceRecord), record.Id.ToString()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void EnsureTenantResolved()
    {
        if (!_tenantContext.IsResolved)
            throw new DomainException("Tenant context is not resolved.");
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord record) =>
        new(
            record.Id,
            record.EmployeeId,
            record.Date,
            record.CheckIn,
            record.CheckOut,
            record.Status.ToString(),
            record.Notes);
}
