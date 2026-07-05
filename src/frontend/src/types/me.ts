export interface TenantPlanFeatures {
  maxEmployees: number;
  customRoles: boolean;
  auditLog: boolean;
  advancedReports: boolean;
}

export interface Me {
  userId: string;
  email: string;
  tenantId: string;
  tenantSlug: string;
  employeeId: string | null;
  roleSlugs: string[];
  permissions: string[];
  features: string[];
  isPlatformAdmin: boolean;
  planFeatures: TenantPlanFeatures;
}
