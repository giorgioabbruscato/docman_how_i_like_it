export interface PlatformDashboardSummary {
  totalTenants: number;
  totalEmployees: number;
  activeEmployeesLast30Days: number;
  totalTimeEntriesLast30Days: number;
  licenseSeatsUsed: number;
  licenseSeatsTotal: number;
}

export interface PlatformTenantMetrics {
  tenantId: string;
  slug: string;
  name: string;
  employeeCount: number;
  isActive: boolean;
  createdAt: string;
  lastActivityAt: string | null;
}

export interface PlatformTenantSummary {
  tenantId: string;
  slug: string;
  name: string;
  employeeCount: number;
  activeProjects: number;
  timeEntriesThisMonth: number;
  attendanceSessionsThisMonth: number;
  leaveRequestsPending: number;
  storageUsedBytes: number | null;
}

export interface PlatformUsageTrendPoint {
  period: string;
  count: number;
}

export interface PlatformUsage {
  tenantGrowth: PlatformUsageTrendPoint[];
  timeEntriesByMonth: PlatformUsageTrendPoint[];
}
