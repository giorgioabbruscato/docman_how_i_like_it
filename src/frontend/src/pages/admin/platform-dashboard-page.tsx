import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  fetchPlatformDashboard,
  fetchPlatformTenants,
  fetchPlatformUsage,
} from '@/api/platformAdmin';
import { PlatformKpiCards } from '@/components/dashboard/platform-kpi-cards';
import { WidgetErrorBoundary } from '@/components/dashboard/widget-primitives';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { getApiErrorMessage } from '@/lib/utils';
import type { PlatformTenantMetrics } from '@/types/platform-admin';
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

type SortKey = 'name' | 'employeeCount' | 'lastActivityAt';
type SortDirection = 'asc' | 'desc';

function compareTenants(
  a: PlatformTenantMetrics,
  b: PlatformTenantMetrics,
  key: SortKey,
  direction: SortDirection,
): number {
  const factor = direction === 'asc' ? 1 : -1;

  if (key === 'name') {
    return a.name.localeCompare(b.name) * factor;
  }

  if (key === 'employeeCount') {
    return (a.employeeCount - b.employeeCount) * factor;
  }

  const aTime = a.lastActivityAt ? new Date(a.lastActivityAt).getTime() : 0;
  const bTime = b.lastActivityAt ? new Date(b.lastActivityAt).getTime() : 0;
  return (aTime - bTime) * factor;
}

function formatDate(value: string | null): string {
  if (!value) return '—';
  return new Date(value).toLocaleString();
}

export function PlatformDashboardPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dashboard, setDashboard] = useState<Awaited<ReturnType<typeof fetchPlatformDashboard>> | null>(
    null,
  );
  const [tenants, setTenants] = useState<PlatformTenantMetrics[]>([]);
  const [usage, setUsage] = useState<Awaited<ReturnType<typeof fetchPlatformUsage>> | null>(null);
  const [sortKey, setSortKey] = useState<SortKey>('name');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const [dashboardData, tenantData, usageData] = await Promise.all([
          fetchPlatformDashboard(),
          fetchPlatformTenants(),
          fetchPlatformUsage(),
        ]);
        setDashboard(dashboardData);
        setTenants(tenantData);
        setUsage(usageData);
      } catch (err) {
        setError(getApiErrorMessage(err, 'Failed to load platform dashboard.'));
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, []);

  const sortedTenants = useMemo(
    () => [...tenants].sort((a, b) => compareTenants(a, b, sortKey, sortDirection)),
    [tenants, sortKey, sortDirection],
  );

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) {
      setSortDirection((current) => (current === 'asc' ? 'desc' : 'asc'));
      return;
    }

    setSortKey(key);
    setSortDirection(key === 'lastActivityAt' ? 'desc' : 'asc');
  };

  const sortIndicator = (key: SortKey) => {
    if (sortKey !== key) return '';
    return sortDirection === 'asc' ? ' ↑' : ' ↓';
  };

  if (loading) {
    return <LoadingSpinner label="Loading platform dashboard…" />;
  }

  if (error || !dashboard) {
    return <ErrorBanner message={error ?? 'Failed to load platform dashboard.'} />;
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Platform Admin</h2>
        <p className="text-muted-foreground">Cross-tenant metrics and tenant health overview.</p>
      </div>

      <WidgetErrorBoundary title="Platform KPIs">
        <PlatformKpiCards summary={dashboard} />
      </WidgetErrorBoundary>

      <Card>
        <CardHeader>
          <CardTitle>Tenants</CardTitle>
        </CardHeader>
        <CardContent className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left">
                <th className="py-2 pr-4">
                  <button type="button" className="font-medium" onClick={() => toggleSort('name')}>
                    Name{sortIndicator('name')}
                  </button>
                </th>
                <th className="py-2 pr-4">Slug</th>
                <th className="py-2 pr-4">
                  <button
                    type="button"
                    className="font-medium"
                    onClick={() => toggleSort('employeeCount')}
                  >
                    Employees{sortIndicator('employeeCount')}
                  </button>
                </th>
                <th className="py-2 pr-4">Status</th>
                <th className="py-2 pr-4">
                  <button
                    type="button"
                    className="font-medium"
                    onClick={() => toggleSort('lastActivityAt')}
                  >
                    Last Activity{sortIndicator('lastActivityAt')}
                  </button>
                </th>
                <th className="py-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {sortedTenants.map((tenant) => (
                <tr key={tenant.tenantId} className="border-b last:border-0">
                  <td className="py-3 pr-4 font-medium">{tenant.name}</td>
                  <td className="py-3 pr-4 text-muted-foreground">{tenant.slug}</td>
                  <td className="py-3 pr-4">{tenant.employeeCount}</td>
                  <td className="py-3 pr-4">{tenant.isActive ? 'Active' : 'Inactive'}</td>
                  <td className="py-3 pr-4">{formatDate(tenant.lastActivityAt)}</td>
                  <td className="py-3">
                    <Link
                      to={`/admin/tenants/${tenant.tenantId}`}
                      className="text-primary hover:underline"
                    >
                      View summary
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </CardContent>
      </Card>

      {usage && (
        <div className="grid gap-4 md:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Tenant Growth (12 months)</CardTitle>
            </CardHeader>
            <CardContent className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={usage.tenantGrowth}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="period" />
                  <YAxis allowDecimals={false} />
                  <Tooltip />
                  <Bar dataKey="count" fill="hsl(var(--primary))" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Time Entries by Month</CardTitle>
            </CardHeader>
            <CardContent className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={usage.timeEntriesByMonth}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="period" />
                  <YAxis allowDecimals={false} />
                  <Tooltip />
                  <Bar dataKey="count" fill="hsl(var(--chart-2, var(--primary)))" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
