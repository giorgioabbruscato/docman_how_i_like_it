namespace HrPortal.AccessControl.Domain;

public static class Permissions
{
    // Employees
    public const string EmployeeReadTenant = "employee.read:tenant";
    public const string EmployeeReadTeam = "employee.read:team";
    public const string EmployeeReadSelf = "employee.read:self";
    public const string EmployeeCreateTenant = "employee.create:tenant";
    public const string EmployeeUpdateTenant = "employee.update:tenant";
    public const string EmployeeDeleteTenant = "employee.delete:tenant";

    // Departments
    public const string DepartmentReadTenant = "department.read:tenant";
    public const string DepartmentWriteTenant = "department.write:tenant";
    public const string DepartmentDeleteTenant = "department.delete:tenant";

    // Leave
    public const string LeaveReadTenant = "leave.read:tenant";
    public const string LeaveReadTeam = "leave.read:team";
    public const string LeaveReadSelf = "leave.read:self";
    public const string LeaveCreateSelf = "leave.create:self";
    public const string LeaveApproveTeam = "leave.approve:team";
    public const string LeaveDeleteSelf = "leave.delete:self";

    // Attendance
    public const string AttendanceReadTenant = "attendance.read:tenant";
    public const string AttendanceReadTeam = "attendance.read:team";
    public const string AttendanceReadSelf = "attendance.read:self";
    public const string AttendanceWriteSelf = "attendance.write:self";

    // Documents
    public const string DocumentReadTenant = "document.read:tenant";
    public const string DocumentReadSelf = "document.read:self";
    public const string DocumentUploadSelf = "document.upload:self";
    public const string DocumentDeleteTenant = "document.delete:tenant";

    // Access control
    public const string RoleReadTenant = "role.read:tenant";
    public const string RoleCreateTenant = "role.create:tenant";
    public const string RoleUpdateTenant = "role.update:tenant";
    public const string RoleDeleteTenant = "role.delete:tenant";
    public const string MembershipReadTenant = "membership.read:tenant";
    public const string MembershipCreateTenant = "membership.create:tenant";
    public const string MembershipUpdateTenant = "membership.update:tenant";
    public const string MembershipDeleteTenant = "membership.delete:tenant";

    // Tasks
    public const string TaskReadTenant = "task.read:tenant";
    public const string TaskCreateTenant = "task.create:tenant";
    public const string TaskUpdateTenant = "task.update:tenant";
    public const string TaskDeleteTenant = "task.delete:tenant";
    public const string TaskUpdateStatusSelf = "task.update_status:self";

    // Time tracking
    public const string TimeEntryReadSelf = "time_entry.read:self";
    public const string TimeEntryReadTeam = "time_entry.read:team";
    public const string TimeEntryReadTenant = "time_entry.read:tenant";
    public const string TimeEntryCreateSelf = "time_entry.create:self";
    public const string TimeEntryUpdateSelf = "time_entry.update:self";
    public const string TimeEntryDeleteSelf = "time_entry.delete:self";
    public const string TimeEntryExportTeam = "time_entry.export:team";
    public const string TimeEntryExportTenant = "time_entry.export:tenant";

    // Projects
    public const string ProjectReadTenant = "project.read:tenant";
    public const string ProjectCreateTenant = "project.create:tenant";
    public const string ProjectUpdateTenant = "project.update:tenant";
    public const string ProjectDeleteTenant = "project.delete:tenant";
    public const string ProjectManageMembersTenant = "project.manage_members:tenant";

    // Audit
    public const string AuditReadTenant = "audit.read:tenant";

    // Platform (scope: all)
    public const string TenantManageAll = "tenant.manage:all";
    public const string BillingManageAll = "billing.manage:all";
    public const string SupportAccessAll = "support.access:all";
    public const string SystemOverrideAll = "system.override:all";

    public static readonly IReadOnlyList<string> PlatformAdmin =
    [
        TenantManageAll,
        BillingManageAll,
        SupportAccessAll,
        SystemOverrideAll
    ];

    public static readonly IReadOnlyList<string> AllTenantScoped =
    [
        EmployeeReadTenant, EmployeeReadTeam, EmployeeReadSelf, EmployeeCreateTenant, EmployeeUpdateTenant, EmployeeDeleteTenant,
        DepartmentReadTenant, DepartmentWriteTenant, DepartmentDeleteTenant,
        LeaveReadTenant, LeaveReadTeam, LeaveReadSelf, LeaveCreateSelf, LeaveApproveTeam, LeaveDeleteSelf,
        AttendanceReadTenant, AttendanceReadTeam, AttendanceReadSelf, AttendanceWriteSelf,
        DocumentReadTenant, DocumentReadSelf, DocumentUploadSelf, DocumentDeleteTenant,
        RoleReadTenant, RoleCreateTenant, RoleUpdateTenant, RoleDeleteTenant,
        MembershipReadTenant, MembershipCreateTenant, MembershipUpdateTenant, MembershipDeleteTenant,
        ProjectReadTenant, ProjectCreateTenant, ProjectUpdateTenant, ProjectDeleteTenant, ProjectManageMembersTenant,
        TaskReadTenant, TaskCreateTenant, TaskUpdateTenant, TaskDeleteTenant, TaskUpdateStatusSelf,
        TimeEntryReadSelf, TimeEntryReadTeam, TimeEntryReadTenant,
        TimeEntryCreateSelf, TimeEntryUpdateSelf, TimeEntryDeleteSelf,
        TimeEntryExportTeam, TimeEntryExportTenant,
        AuditReadTenant
    ];
}
