/**
 * @deprecated Task 23: UI gating has moved to permission checks against `/api/v1/me`.
 * Use `hasPermission` / `hasAnyPermission` from `@/lib/auth-permissions` instead.
 * Retained only for display of raw Keycloak realm roles (e.g. the settings page).
 */
export const hasAnyRole = (userRoles: string[], ...required: string[]): boolean =>
  required.some((role) => userRoles.includes(role));

/** @deprecated Task 23: use `Permission.DocumentDeleteTenant` / role-derived permissions instead. */
export const HR_OR_ADMIN_ROLES = ['Admin', 'HR'] as const;
/** @deprecated Task 23: use `hasAnyPermission` with the relevant `*ReadTenant`/`*ReadTeam` permissions instead. */
export const MANAGER_OR_ABOVE_ROLES = ['Admin', 'HR', 'Manager'] as const;
