using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.Employees.Application;
using HrPortal.Workflows.Application;
using HrPortal.Workflows.Domain;

namespace HrPortal.Workflows.Infrastructure;

internal sealed class WorkflowApproverResolver : IWorkflowApproverResolver
{
    private readonly IEmployeeLookup _employeeLookup;
    private readonly ITenantMembershipRepository _membershipRepository;
    private readonly ITenantRoleRepository _roleRepository;

    public WorkflowApproverResolver(
        IEmployeeLookup employeeLookup,
        ITenantMembershipRepository membershipRepository,
        ITenantRoleRepository roleRepository)
    {
        _employeeLookup = employeeLookup;
        _membershipRepository = membershipRepository;
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyList<WorkflowApprover>> ResolveApproversAsync(
        WorkflowStepDefinition step,
        Guid requesterEmployeeId,
        CancellationToken cancellationToken = default)
    {
        return step.ApproverType switch
        {
            ApproverType.DirectManager => await ResolveDirectManagersAsync(requesterEmployeeId, cancellationToken),
            ApproverType.Role => await ResolveByRoleAsync(step.Role, cancellationToken),
            ApproverType.NamedEmployee when step.EmployeeId.HasValue =>
                await ResolveNamedEmployeeAsync(step.EmployeeId.Value, cancellationToken),
            _ => []
        };
    }

    private async Task<IReadOnlyList<WorkflowApprover>> ResolveDirectManagersAsync(
        Guid requesterEmployeeId,
        CancellationToken cancellationToken)
    {
        var departmentMap = await _employeeLookup.GetDepartmentIdsAsync([requesterEmployeeId], cancellationToken);
        if (!departmentMap.TryGetValue(requesterEmployeeId, out var departmentId) || !departmentId.HasValue)
            return [];

        var managerRole = await _roleRepository.GetBySlugAsync(SystemRoleTemplates.ManagerSlug, cancellationToken);
        if (managerRole is null)
            return [];

        var departmentKey = departmentId.Value.ToString();
        var memberships = await _membershipRepository.GetAllAsync(cancellationToken);
        var approvers = new List<WorkflowApprover>();

        foreach (var membership in memberships.Where(m => m.IsActive))
        {
            var attrs = membership.GetAttributes();
            if (!attrs.TryGetValue("departmentId", out var attrDept) || attrDept != departmentKey)
                continue;

            if (!membership.GetRoleIds().Contains(managerRole.Id))
                continue;

            approvers.Add(new WorkflowApprover(membership.UserId, membership.EmployeeId));
        }

        return approvers;
    }

    private async Task<IReadOnlyList<WorkflowApprover>> ResolveByRoleAsync(
        string? permission,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return [];

        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        var matchingRoleIds = roles
            .Where(r => r.IsActive && r.GetPermissions().Contains(permission, StringComparer.Ordinal))
            .Select(r => r.Id)
            .ToHashSet();

        if (matchingRoleIds.Count == 0)
            return [];

        var memberships = await _membershipRepository.GetAllAsync(cancellationToken);
        return memberships
            .Where(m => m.IsActive && m.GetRoleIds().Any(matchingRoleIds.Contains))
            .Select(m => new WorkflowApprover(m.UserId, m.EmployeeId))
            .DistinctBy(a => a.UserId)
            .ToList();
    }

    private async Task<IReadOnlyList<WorkflowApprover>> ResolveNamedEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var membership = await _membershipRepository.GetActiveByEmployeeIdAsync(employeeId, cancellationToken);
        return membership is null
            ? []
            : [new WorkflowApprover(membership.UserId, employeeId)];
    }
}
