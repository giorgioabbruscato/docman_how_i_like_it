import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { fetchPlatformTenantSummary } from '@/api/platformAdmin';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { getApiErrorMessage } from '@/lib/utils';
import type { PlatformTenantSummary } from '@/types/platform-admin';

function formatStorage(bytes: number | null): string {
  if (bytes == null) return 'Not tracked';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  return `${(bytes / (1024 * 1024 * 1024)).toFixed(1)} GB`;
}

export function TenantSummaryPage() {
  const { tenantId } = useParams<{ tenantId: string }>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [summary, setSummary] = useState<PlatformTenantSummary | null>(null);

  useEffect(() => {
    if (!tenantId) return;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await fetchPlatformTenantSummary(tenantId);
        setSummary(data);
      } catch (err) {
        setError(getApiErrorMessage(err, 'Failed to load tenant summary.'));
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [tenantId]);

  if (!tenantId) {
    return <ErrorBanner message="Tenant id is required." />;
  }

  if (loading) {
    return <LoadingSpinner label="Loading tenant summary…" />;
  }

  if (error || !summary) {
    return <ErrorBanner message={error ?? 'Failed to load tenant summary.'} />;
  }

  const metrics = [
    { label: 'Employees', value: summary.employeeCount.toLocaleString() },
    { label: 'Active Projects', value: summary.activeProjects.toLocaleString() },
    { label: 'Time Entries (this month)', value: summary.timeEntriesThisMonth.toLocaleString() },
    {
      label: 'Attendance Sessions (this month)',
      value: summary.attendanceSessionsThisMonth.toLocaleString(),
    },
    { label: 'Pending Leave Requests', value: summary.leaveRequestsPending.toLocaleString() },
    { label: 'Storage Used', value: formatStorage(summary.storageUsedBytes) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-sm text-muted-foreground">
            <Link to="/admin/dashboard" className="hover:underline">
              Platform Admin
            </Link>
            {' / '}
            Tenant Summary
          </p>
          <h2 className="text-3xl font-bold tracking-tight">{summary.name}</h2>
          <p className="text-muted-foreground">Slug: {summary.slug}</p>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {metrics.map((metric) => (
          <Card key={metric.label}>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {metric.label}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-2xl font-bold">{metric.value}</p>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
