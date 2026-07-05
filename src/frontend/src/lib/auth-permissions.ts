import { useAuthStore } from '@/stores/auth-store';

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

  AttendanceSessionReadSelf: 'attendance_session.read:self',
  AttendanceSessionReadTeam: 'attendance_session.read:team',
  AttendanceSessionReadTenant: 'attendance_session.read:tenant',
  AttendanceSessionCheckInSelf: 'attendance_session.check_in:self',
  AttendanceSessionCheckOutSelf: 'attendance_session.check_out:self',

  ProjectReadTenant: 'project.read:tenant',
  ProjectCreateTenant: 'project.create:tenant',
  ProjectUpdateTenant: 'project.update:tenant',
  ProjectDeleteTenant: 'project.delete:tenant',
  ProjectManageMembersTenant: 'project.manage_members:tenant',

  TaskReadTenant: 'task.read:tenant',

  TimeEntryReadSelf: 'time_entry.read:self',
  TimeEntryReadTeam: 'time_entry.read:team',
  TimeEntryReadTenant: 'time_entry.read:tenant',
  TimeEntryCreateSelf: 'time_entry.create:self',
  TimeEntryUpdateSelf: 'time_entry.update:self',
  TimeEntryDeleteSelf: 'time_entry.delete:self',

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

export function useHasPermission(required: PermissionKey): boolean {
  const permissions = useAuthStore((state) => state.permissions);
  return hasPermission(permissions, required);
}

export function useHasAnyPermission(...required: PermissionKey[]): boolean {
  const permissions = useAuthStore((state) => state.permissions);
  return hasAnyPermission(permissions, ...required);
}
