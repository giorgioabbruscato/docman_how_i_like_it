import { useEffect, useState } from 'react';
import { fetchDocuments } from '@/api/documents';
import { fetchEmployees } from '@/api/employees';
import { fetchLeaveRequests } from '@/api/leave-requests';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';

interface DashboardStats {
  activeEmployees?: number;
  pendingLeave?: number;
  documentCount?: number;
}

export function DashboardPage() {
  const permissions = useAuthStore((state) => state.permissions);
  const canReadEmployees = hasAnyPermission(
    permissions,
    Permission.EmployeeReadTenant,
    Permission.EmployeeReadTeam,
  );
  const canReadLeave = hasAnyPermission(
    permissions,
    Permission.LeaveReadTenant,
    Permission.LeaveReadTeam,
  );
  const canReadDocuments = hasAnyPermission(permissions, Permission.DocumentReadTenant);

  const [stats, setStats] = useState<DashboardStats>({});
  const [loadingEmployees, setLoadingEmployees] = useState(false);
  const [loadingLeave, setLoadingLeave] = useState(false);
  const [loadingDocuments, setLoadingDocuments] = useState(false);

  useEffect(() => {
    if (!canReadEmployees) return;

    const loadEmployees = async () => {
      setLoadingEmployees(true);
      try {
        const employees = await fetchEmployees();
        setStats((prev) => ({
          ...prev,
          activeEmployees: employees.filter((e) => e.isActive).length,
        }));
      } catch {
        setStats((prev) => ({ ...prev, activeEmployees: undefined }));
      } finally {
        setLoadingEmployees(false);
      }
    };

    void loadEmployees();
  }, [canReadEmployees]);

  useEffect(() => {
    if (!canReadLeave) return;

    const loadLeave = async () => {
      setLoadingLeave(true);
      try {
        const leaveRequests = await fetchLeaveRequests();
        setStats((prev) => ({
          ...prev,
          pendingLeave: leaveRequests.filter((r) => r.status === 'Pending').length,
        }));
      } catch {
        setStats((prev) => ({ ...prev, pendingLeave: undefined }));
      } finally {
        setLoadingLeave(false);
      }
    };

    void loadLeave();
  }, [canReadLeave]);

  useEffect(() => {
    if (!canReadDocuments) return;

    const loadDocuments = async () => {
      setLoadingDocuments(true);
      try {
        const documents = await fetchDocuments();
        setStats((prev) => ({ ...prev, documentCount: documents.length }));
      } catch {
        setStats((prev) => ({ ...prev, documentCount: undefined }));
      } finally {
        setLoadingDocuments(false);
      }
    };

    void loadDocuments();
  }, [canReadDocuments]);

  const display = (
    canRead: boolean,
    loading: boolean,
    value: number | undefined,
  ) => {
    if (!canRead) return '—';
    if (loading) return '…';
    if (value === undefined) return '—';
    return String(value);
  };

  const isLoading = loadingEmployees || loadingLeave || loadingDocuments;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
        <p className="text-muted-foreground">Overview of your HR platform.</p>
      </div>

      {isLoading && <LoadingSpinner label="Loading dashboard" />}

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Employees</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">
              {display(canReadEmployees, loadingEmployees, stats.activeEmployees)}
            </p>
            <p className="text-xs text-muted-foreground">Active employees</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Leave Requests</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">
              {display(canReadLeave, loadingLeave, stats.pendingLeave)}
            </p>
            <p className="text-xs text-muted-foreground">Pending approval</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Documents</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">
              {display(canReadDocuments, loadingDocuments, stats.documentCount)}
            </p>
            <p className="text-xs text-muted-foreground">Total uploaded</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
