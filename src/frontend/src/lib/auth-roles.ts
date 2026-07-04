export const hasAnyRole = (userRoles: string[], ...required: string[]): boolean =>
  required.some((role) => userRoles.includes(role));

export const HR_OR_ADMIN_ROLES = ['Admin', 'HR'] as const;
export const MANAGER_OR_ABOVE_ROLES = ['Admin', 'HR', 'Manager'] as const;
