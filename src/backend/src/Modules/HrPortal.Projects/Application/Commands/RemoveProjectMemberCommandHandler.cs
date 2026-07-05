using HrPortal.Audit.Application;
using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Projects.Application.Commands;

public sealed class RemoveProjectMemberCommandHandler
{
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<RemoveProjectMemberCommandHandler> _logger;

    public RemoveProjectMemberCommandHandler(
        IProjectMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<RemoveProjectMemberCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(
        Guid projectId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member is null || member.ProjectId != projectId)
            return Result.Failure("Project member not found.", "NOT_FOUND");

        await _memberRepository.RemoveAsync(member, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "project.member.removed",
            nameof(ProjectMember),
            member.Id.ToString(),
            $"{{\"projectId\":\"{projectId}\"}}"), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Member {MemberId} removed from project {ProjectId}", memberId, projectId);
        return Result.Success();
    }
}
