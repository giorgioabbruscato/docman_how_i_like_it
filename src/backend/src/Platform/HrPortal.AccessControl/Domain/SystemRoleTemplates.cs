namespace HrPortal.AccessControl.Domain;

public static class SystemRoleTemplates
{
    public const string AdminSlug = "admin";
    public const string HrSlug = "hr";
    public const string ManagerSlug = "manager";
    public const string EmployeeSlug = "employee";

    public static readonly IReadOnlyList<string> AllSlugs =
        [AdminSlug, HrSlug, ManagerSlug, EmployeeSlug];

    public static IReadOnlyList<string> GetPermissions(string slug) =>
        slug.ToLowerInvariant() switch
        {
            AdminSlug => Permissions.AllTenantScoped,
            HrSlug => HrPermissions,
            ManagerSlug => ManagerPermissions,
            EmployeeSlug => EmployeePermissions,
            _ => []
        };

    private static readonly IReadOnlyList<string> HrPermissions =
    [
        Permissions.EmployeeReadTenant,
        Permissions.EmployeeCreateTenant,
        Permissions.EmployeeUpdateTenant,
        Permissions.EmployeeDeleteTenant,
        Permissions.DepartmentReadTenant,
        Permissions.DepartmentWriteTenant,
        Permissions.DepartmentDeleteTenant,
        Permissions.LeaveReadTenant,
        Permissions.LeaveCreateSelf,
        Permissions.LeaveApproveTeam,
        Permissions.AttendanceSessionReadTeam,
        Permissions.AttendanceSessionCheckInSelf,
        Permissions.AttendanceSessionCheckOutSelf,
        Permissions.DocumentReadTenant,
        Permissions.DocumentUploadSelf,
        Permissions.DocumentDeleteTenant,
        Permissions.MembershipReadTenant,
        Permissions.MembershipCreateTenant,
        Permissions.MembershipUpdateTenant,
        Permissions.RoleReadTenant,
        Permissions.AuditReadTenant,
        Permissions.ProjectReadTenant,
        Permissions.ProjectCreateTenant,
        Permissions.ProjectUpdateTenant,
        Permissions.ProjectDeleteTenant,
        Permissions.ProjectManageMembersTenant,
        Permissions.TaskReadTenant,
        Permissions.TaskCreateTenant,
        Permissions.TaskUpdateTenant,
        Permissions.TaskDeleteTenant,
        Permissions.TimeEntryReadTenant,
        Permissions.TimeEntryCreateSelf,
        Permissions.TimeEntryUpdateSelf,
        Permissions.TimeEntryDeleteSelf,
        Permissions.TimeEntryExportTenant,
        Permissions.AnalyticsReadTenant
    ];

    private static readonly IReadOnlyList<string> ManagerPermissions =
    [
        Permissions.EmployeeReadTeam,
        Permissions.LeaveReadTeam,
        Permissions.LeaveApproveTeam,
        Permissions.AttendanceSessionReadTeam,
        Permissions.DocumentReadTenant,
        Permissions.EmployeeReadSelf,
        Permissions.LeaveReadSelf,
        Permissions.LeaveCreateSelf,
        Permissions.LeaveDeleteSelf,
        Permissions.AttendanceSessionReadSelf,
        Permissions.AttendanceSessionCheckInSelf,
        Permissions.AttendanceSessionCheckOutSelf,
        Permissions.DocumentReadSelf,
        Permissions.DocumentUploadSelf,
        Permissions.TaskUpdateStatusSelf,
        Permissions.TimeEntryReadTeam,
        Permissions.TimeEntryReadSelf,
        Permissions.TimeEntryCreateSelf,
        Permissions.TimeEntryUpdateSelf,
        Permissions.TimeEntryDeleteSelf,
        Permissions.TimeEntryExportTeam,
        Permissions.AnalyticsReadTeam
    ];

    private static readonly IReadOnlyList<string> EmployeePermissions =
    [
        Permissions.EmployeeReadSelf,
        Permissions.LeaveReadSelf,
        Permissions.LeaveCreateSelf,
        Permissions.LeaveDeleteSelf,
        Permissions.AttendanceSessionReadSelf,
        Permissions.AttendanceSessionCheckInSelf,
        Permissions.AttendanceSessionCheckOutSelf,
        Permissions.DocumentReadSelf,
        Permissions.DocumentUploadSelf,
        Permissions.TaskUpdateStatusSelf,
        Permissions.TimeEntryReadSelf,
        Permissions.TimeEntryCreateSelf,
        Permissions.TimeEntryUpdateSelf,
        Permissions.TimeEntryDeleteSelf
    ];

}
