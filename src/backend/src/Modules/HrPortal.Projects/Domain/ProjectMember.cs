using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Projects.Domain;

public sealed class ProjectMember : AuditableEntity
{
    public Guid ProjectId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public ProjectMemberRole Role { get; private set; }
    public decimal? HourlyRate { get; private set; }

    private ProjectMember() { }

    public static ProjectMember Create(
        Guid tenantId,
        Guid projectId,
        Guid employeeId,
        ProjectMemberRole role,
        decimal? hourlyRate = null,
        Guid? createdBy = null)
    {
        if (hourlyRate.HasValue && hourlyRate.Value < 0)
            throw new DomainException("Hourly rate must be greater than or equal to zero.");

        return new ProjectMember
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            Role = role,
            HourlyRate = hourlyRate,
            CreatedBy = createdBy
        }.Also(m => m.SetTenant(tenantId));
    }
}
