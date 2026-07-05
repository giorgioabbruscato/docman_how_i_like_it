/** Mirrors backend permission strings in `Permissions.cs`. */
export const Permission = {
  EmployeeReadTenant: 'employee.read:tenant',
  EmployeeReadTeam: 'employee.read:team',
  EmployeeReadSelf: 'employee.read:self',
  EmployeeCreateTenant: 'employee.create:tenant',
  EmployeeUpdateTenant: 'employee.update:tenant',
  EmployeeDeleteTenant: 'employee.delete:tenant',

  DepartmentReadTenant: 'department.read:tenant',
  DepartmentWriteTenant: 'department.write:tenant',
  DepartmentDeleteTenant: 'department.delete:tenant',

  LeaveReadTenant: 'leave.read:tenant',
  LeaveReadTeam: 'leave.read:team',
  LeaveReadSelf: 'leave.read:self',
  LeaveCreateSelf: 'leave.create:self',
  LeaveApproveTeam: 'leave.approve:team',
  LeaveDeleteSelf: 'leave.delete:self',

  AttendanceReadTenant: 'attendance.read:tenant',
  AttendanceReadTeam: 'attendance.read:team',
  AttendanceReadSelf: 'attendance.read:self',
  AttendanceWriteSelf: 'attendance.write:self',

  DocumentReadTenant: 'document.read:tenant',
  DocumentReadSelf: 'document.read:self',
  DocumentUploadSelf: 'document.upload:self',
  DocumentDeleteTenant: 'document.delete:tenant',

  RoleReadTenant: 'role.read:tenant',
  RoleCreateTenant: 'role.create:tenant',
  RoleUpdateTenant: 'role.update:tenant',
  RoleDeleteTenant: 'role.delete:tenant',
  MembershipReadTenant: 'membership.read:tenant',
  MembershipCreateTenant: 'membership.create:tenant',
  MembershipUpdateTenant: 'membership.update:tenant',
  MembershipDeleteTenant: 'membership.delete:tenant',

  AuditReadTenant: 'audit.read:tenant',

  TenantManageAll: 'tenant.manage:all',
  BillingManageAll: 'billing.manage:all',
  SupportAccessAll: 'support.access:all',
  SystemOverrideAll: 'system.override:all',
} as const;

export type PermissionKey = (typeof Permission)[keyof typeof Permission];

export const hasPermission = (permissions: string[], required: string): boolean =>
  permissions.includes(required);

export const hasAnyPermission = (permissions: string[], ...required: string[]): boolean =>
  required.some((permission) => permissions.includes(permission));
