import { Link, Outlet } from 'react-router-dom';
import { keycloak } from '@/lib/keycloak';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { isSingleTenancyMode } from '@/lib/tenancy-config';
import { useAuthStore } from '@/stores/auth-store';
import { Button } from '@/components/ui/button';
import type { TenantPlanFeatures } from '@/types/me';
import {
  Building2,
  CalendarDays,
  Clock,
  FileText,
  LayoutDashboard,
  ScrollText,
  Settings,
  Users,
} from 'lucide-react';

interface NavItem {
  to: string;
  label: string;
  icon: typeof LayoutDashboard;
  isVisible: (permissions: string[], planFeatures: TenantPlanFeatures) => boolean;
}

const navItems: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, isVisible: () => true },
  {
    to: '/departments',
    label: 'Departments',
    icon: Building2,
    isVisible: (permissions) => hasAnyPermission(permissions, Permission.DepartmentReadTenant),
  },
  {
    to: '/employees',
    label: 'Employees',
    icon: Users,
    isVisible: (permissions) =>
      hasAnyPermission(permissions, Permission.EmployeeReadTenant, Permission.EmployeeReadTeam),
  },
  { to: '/leave-requests', label: 'Leave Requests', icon: CalendarDays, isVisible: () => true },
  {
    to: '/attendance',
    label: 'Attendance',
    icon: Clock,
    isVisible: (permissions) =>
      hasAnyPermission(
        permissions,
        Permission.AttendanceReadTenant,
        Permission.AttendanceReadTeam,
        Permission.AttendanceReadSelf,
      ),
  },
  { to: '/documents', label: 'Documents', icon: FileText, isVisible: () => true },
  {
    to: '/audit-logs',
    label: 'Audit Logs',
    icon: ScrollText,
    isVisible: (permissions, planFeatures) =>
      hasAnyPermission(permissions, Permission.AuditReadTenant) && planFeatures.auditLog,
  },
  { to: '/settings', label: 'Settings', icon: Settings, isVisible: () => true },
];

export function AppLayout() {
  const { user, logout, permissions, planFeatures, me } = useAuthStore();
  const tenantDisplay = me?.tenantSlug ?? import.meta.env.VITE_TENANT_ID ?? 'demo';

  const handleLogout = () => {
    logout();
    void keycloak.logout({ redirectUri: `${window.location.origin}/login` });
  };

  const visibleNavItems = navItems.filter((item) => item.isVisible(permissions, planFeatures));

  return (
    <div className="min-h-screen flex">
      <aside className="w-64 border-r border-border bg-muted/30 p-4">
        <div className="mb-8">
          <h1 className="text-xl font-bold">HR Portal</h1>
          {!isSingleTenancyMode && (
            <p className="text-sm text-muted-foreground">Multi-tenant platform</p>
          )}
        </div>
        <nav className="space-y-1">
          {visibleNavItems.map(({ to, label, icon: Icon }) => (
            <Link
              key={to}
              to={to}
              className="flex items-center gap-2 rounded-md px-3 py-2 text-sm hover:bg-muted"
            >
              <Icon className="h-4 w-4" />
              {label}
            </Link>
          ))}
        </nav>
      </aside>

      <div className="flex-1 flex flex-col">
        <header className="h-14 border-b border-border flex items-center justify-between px-6">
          {!isSingleTenancyMode ? (
            <span className="text-sm text-muted-foreground">
              Tenant: <strong>{tenantDisplay}</strong>
            </span>
          ) : (
            <span />
          )}
          <div className="flex items-center gap-3">
            {user ? (
              <>
                <div className="text-right">
                  <p className="text-sm">{user.name}</p>
                  {user.roles.length > 0 && (
                    <p className="text-xs text-muted-foreground">{user.roles.join(', ')}</p>
                  )}
                </div>
                <Button variant="outline" size="sm" onClick={handleLogout}>
                  Logout
                </Button>
              </>
            ) : (
              <span className="text-sm text-muted-foreground">Not authenticated</span>
            )}
          </div>
        </header>
        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
