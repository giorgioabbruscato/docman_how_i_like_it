import {
  AnalyticsFilters,
  useAnalyticsFilters,
} from '@/components/analytics/analytics-filters';
import { AttendanceTrendChart } from '@/components/analytics/attendance-trend-chart';
import { BudgetConsumptionChart } from '@/components/analytics/budget-consumption-chart';
import { EmployeesWorkingNow } from '@/components/analytics/employees-working-now';
import { ExportButtons } from '@/components/analytics/export-buttons';
import { HoursByDepartmentChart } from '@/components/analytics/hours-by-department-chart';
import { HoursByEmployeeChart } from '@/components/analytics/hours-by-employee-chart';
import { HoursByProjectChart } from '@/components/analytics/hours-by-project-chart';
import { KpiCards } from '@/components/analytics/kpi-cards';
import { LeaveTrendChart } from '@/components/analytics/leave-trend-chart';
import { MonthlyTrendChart } from '@/components/analytics/monthly-trend-chart';
import { TopEmployeesWidget } from '@/components/analytics/top-employees-widget';
import { TopProjectsWidget } from '@/components/analytics/top-projects-widget';
import { Permission, hasAnyPermission, hasPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';

export function AnalyticsPage() {
  const permissions = useAuthStore((state) => state.permissions);
  const planFeatures = useAuthStore((state) => state.planFeatures);
  const { filters } = useAnalyticsFilters();

  const canViewAnalytics =
    hasAnyPermission(permissions, Permission.AnalyticsReadTeam, Permission.AnalyticsReadTenant) &&
    planFeatures.advancedReports;

  const canViewTenantAnalytics = hasPermission(permissions, Permission.AnalyticsReadTenant);

  if (!canViewAnalytics) {
    return (
      <div className="space-y-6">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Analytics</h2>
          <p className="text-muted-foreground">
            You do not have access to analytics. This feature requires analytics.read:team or
            analytics.read:tenant permission and an Enterprise plan with advanced reports enabled.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Analytics</h2>
        <p className="text-muted-foreground">
          Supervisor dashboard with KPIs, charts, and exportable reports.
        </p>
      </div>

      <AnalyticsFilters />

      <KpiCards filters={filters} showBudget={canViewTenantAnalytics} />

      <div className="grid gap-4 md:grid-cols-2">
        <HoursByProjectChart filters={filters} />
        <HoursByDepartmentChart filters={filters} />
        <HoursByEmployeeChart filters={filters} />
        <MonthlyTrendChart filters={filters} />
        <AttendanceTrendChart filters={filters} />
        <LeaveTrendChart filters={filters} />
        {canViewTenantAnalytics && <BudgetConsumptionChart filters={filters} />}
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <EmployeesWorkingNow filters={filters} />
        <TopEmployeesWidget filters={filters} />
        <TopProjectsWidget filters={filters} />
      </div>

      <ExportButtons filters={filters} />
    </div>
  );
}
