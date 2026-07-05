using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Projects.Application.Commands;

public sealed class AddProjectMemberCommandHandler
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<AddProjectMemberCommandHandler> _logger;

    public AddProjectMemberCommandHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<AddProjectMemberCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _employeeLookup = employeeLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<ProjectMemberDto>> HandleAsync(
        Guid projectId,
        AddProjectMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _projectRepository.ExistsAsync(projectId, cancellationToken))
            return Result.Failure<ProjectMemberDto>("Project not found.", "NOT_FOUND");

        if (!await _employeeLookup.ExistsAndIsActiveAsync(request.EmployeeId, cancellationToken))
            return Result.Failure<ProjectMemberDto>("Employee not found or inactive.", "NOT_FOUND");

        if (await _memberRepository.ExistsAsync(projectId, request.EmployeeId, cancellationToken))
            return Result.Failure<ProjectMemberDto>("Employee is already assigned to this project.", "CONFLICT");

        try
        {
            var member = ProjectMember.Create(
                _tenantContext.TenantId,
                projectId,
                request.EmployeeId,
                request.Role,
                request.HourlyRate,
                _tenantContext.UserId);

            await _memberRepository.AddAsync(member, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "project.member.added",
                nameof(ProjectMember),
                member.Id.ToString(),
                $"{{\"projectId\":\"{projectId}\"}}"), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Member {MemberId} added to project {ProjectId}", member.Id, projectId);
            return Result.Success(ProjectMapping.ToDto(member));
        }
        catch (DomainException ex)
        {
            return Result.Failure<ProjectMemberDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
