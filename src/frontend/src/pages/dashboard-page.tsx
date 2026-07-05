import { AdminKpiCards } from '@/components/dashboard/admin-kpi-cards';
import { DashboardGrid } from '@/components/dashboard/dashboard-grid';
import { NotificationsWidget } from '@/components/dashboard/notifications-widget';
import { WidgetErrorBoundary } from '@/components/dashboard/widget-primitives';
import { WorkedHoursWidget } from '@/components/dashboard/worked-hours-widget';
import { LeaveBalanceWidget } from '@/components/dashboard/leave-balance-widget';
import { AttendanceTodayWidget } from '@/components/dashboard/attendance-today-widget';
import { DocumentsWidget } from '@/components/dashboard/documents-widget';
import { TasksWidget } from '@/components/dashboard/tasks-widget';
import { QuickActionsBar } from '@/components/dashboard/quick-actions-bar';

export function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
        <p className="text-muted-foreground">Your personal HR overview.</p>
      </div>

      <QuickActionsBar />

      <WidgetErrorBoundary title="Admin KPIs">
        <AdminKpiCards />
      </WidgetErrorBoundary>

      <DashboardGrid>
        <WidgetErrorBoundary title="Worked hours">
          <WorkedHoursWidget />
        </WidgetErrorBoundary>
        <WidgetErrorBoundary title="Leave balance">
          <LeaveBalanceWidget />
        </WidgetErrorBoundary>
        <WidgetErrorBoundary title="Attendance today">
          <AttendanceTodayWidget />
        </WidgetErrorBoundary>
        <WidgetErrorBoundary title="Notifications">
          <NotificationsWidget />
        </WidgetErrorBoundary>
        <WidgetErrorBoundary title="Recent documents">
          <DocumentsWidget />
        </WidgetErrorBoundary>
        <WidgetErrorBoundary title="Assigned tasks">
          <TasksWidget />
        </WidgetErrorBoundary>
      </DashboardGrid>
    </div>
  );
}
