import { useEffect, useState } from 'react';
import { fetchDocuments } from '@/api/documents';
import { fetchEmployees } from '@/api/employees';
import { fetchLeaveRequests } from '@/api/leave-requests';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';

interface DashboardStats {
  activeEmployees?: number;
  pendingLeave?: number;
  documentCount?: number;
}

export function AdminKpiCards() {
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

  useEffect(() => {
    if (!canReadEmployees) return;
    void fetchEmployees()
      .then((employees) =>
        setStats((prev) => ({
          ...prev,
          activeEmployees: employees.filter((e) => e.isActive).length,
        })),
      )
      .catch(() => undefined);
  }, [canReadEmployees]);

  useEffect(() => {
    if (!canReadLeave) return;
    void fetchLeaveRequests()
      .then((leaveRequests) =>
        setStats((prev) => ({
          ...prev,
          pendingLeave: leaveRequests.filter((r) => r.status === 'Pending').length,
        })),
      )
      .catch(() => undefined);
  }, [canReadLeave]);

  useEffect(() => {
    if (!canReadDocuments) return;
    void fetchDocuments()
      .then((documents) => setStats((prev) => ({ ...prev, documentCount: documents.length })))
      .catch(() => undefined);
  }, [canReadDocuments]);

  if (!canReadEmployees && !canReadLeave && !canReadDocuments) return null;

  return (
    <div className="grid gap-4 md:grid-cols-3">
      {canReadEmployees && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Employees</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.activeEmployees ?? '—'}</p>
          </CardContent>
        </Card>
      )}
      {canReadLeave && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Pending Leave</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.pendingLeave ?? '—'}</p>
          </CardContent>
        </Card>
      )}
      {canReadDocuments && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Documents</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.documentCount ?? '—'}</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
