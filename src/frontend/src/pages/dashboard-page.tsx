import { useEffect, useState } from 'react';
import { fetchDocuments } from '@/api/documents';
import { fetchEmployees } from '@/api/employees';
import { fetchLeaveRequests } from '@/api/leave-requests';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';

interface DashboardStats {
  activeEmployees: number;
  pendingLeave: number;
  documentCount: number;
}

export function DashboardPage() {
  const permissions = useAuthStore((state) => state.permissions);
  const isManagerOrAbove = hasAnyPermission(
    permissions,
    Permission.EmployeeReadTenant,
    Permission.EmployeeReadTeam,
  );
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!isManagerOrAbove) return;

    const loadStats = async () => {
      setLoading(true);
      try {
        const [employees, leaveRequests, documents] = await Promise.all([
          fetchEmployees(),
          fetchLeaveRequests(),
          fetchDocuments(),
        ]);
        setStats({
          activeEmployees: employees.filter((e) => e.isActive).length,
          pendingLeave: leaveRequests.filter((r) => r.status === 'Pending').length,
          documentCount: documents.length,
        });
      } catch {
        setStats(null);
      } finally {
        setLoading(false);
      }
    };

    void loadStats();
  }, [isManagerOrAbove]);

  const display = (value: number | undefined) => {
    if (loading) return '…';
    if (!isManagerOrAbove || value === undefined) return '—';
    return String(value);
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
        <p className="text-muted-foreground">Overview of your HR platform.</p>
      </div>

      {loading && <LoadingSpinner label="Loading dashboard" />}

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Employees</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{display(stats?.activeEmployees)}</p>
            <p className="text-xs text-muted-foreground">Active employees</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Leave Requests</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{display(stats?.pendingLeave)}</p>
            <p className="text-xs text-muted-foreground">Pending approval</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Documents</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{display(stats?.documentCount)}</p>
            <p className="text-xs text-muted-foreground">Total uploaded</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
