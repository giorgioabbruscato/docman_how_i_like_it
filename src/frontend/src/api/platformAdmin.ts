import { apiClient } from '@/lib/api-client';
import type {
  PlatformDashboardSummary,
  PlatformTenantMetrics,
  PlatformTenantSummary,
  PlatformUsage,
} from '@/types/platform-admin';

export async function fetchPlatformDashboard(): Promise<PlatformDashboardSummary> {
  const { data } = await apiClient.get<PlatformDashboardSummary>('/v1/platform/admin/dashboard');
  return data;
}

export async function fetchPlatformTenants(): Promise<PlatformTenantMetrics[]> {
  const { data } = await apiClient.get<PlatformTenantMetrics[]>('/v1/platform/admin/tenants');
  return data;
}

export async function fetchPlatformTenantSummary(tenantId: string): Promise<PlatformTenantSummary> {
  const { data } = await apiClient.get<PlatformTenantSummary>(
    `/v1/platform/admin/tenants/${tenantId}/summary`,
  );
  return data;
}

export async function fetchPlatformUsage(): Promise<PlatformUsage> {
  const { data } = await apiClient.get<PlatformUsage>('/v1/platform/admin/usage');
  return data;
}
