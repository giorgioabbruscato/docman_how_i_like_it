import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { PlatformAdminRoute } from '@/components/auth/platform-admin-route';
import { ProtectedRoute } from '@/components/auth/protected-route';
import { AppLayout } from '@/components/layout/app-layout';
import { PlatformDashboardPage } from '@/pages/admin/platform-dashboard-page';
import { TenantSummaryPage } from '@/pages/admin/tenant-summary-page';
import { AttendancePage } from '@/pages/attendance-page';
import { AuditPage } from '@/pages/audit-page';
import { DashboardPage } from '@/pages/dashboard-page';
import { DepartmentsPage } from '@/pages/departments-page';
import { DocumentsPage } from '@/pages/documents-page';
import { EmployeesPage } from '@/pages/employees-page';
import { LeaveRequestsPage } from '@/pages/leave-requests-page';
import { LoginPage } from '@/pages/login-page';
import { ProjectCreatePage } from '@/pages/projects/project-create-page';
import { ProjectDetailPage } from '@/pages/projects/project-detail-page';
import { ProjectEditPage } from '@/pages/projects/project-edit-page';
import { ProjectListPage } from '@/pages/projects/project-list-page';
import { SettingsPage } from '@/pages/settings-page';
import { CalendarPage } from '@/pages/time-tracking/calendar-page';
import { ManualEntryPage } from '@/pages/time-tracking/manual-entry-page';
import { TimeTrackingPage } from '@/pages/time-tracking/time-tracking-page';
import { TimesheetsPage } from '@/pages/time-tracking/timesheets/timesheets-page';
import { TimesheetApprovalsPage } from '@/pages/time-tracking/timesheets/timesheet-approvals-page';
import { TeamCalendarPage } from '@/pages/calendar/team-calendar-page';
import { HolidaysPage } from '@/pages/calendar/holidays-page';
import { GeofencingSettingsPage } from '@/pages/settings/geofencing-settings-page';
import { CalendarCallbackPage } from '@/pages/settings/calendar-callback-page';
import { AnalyticsPage } from '@/pages/analytics/analytics-page';

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="departments" element={<DepartmentsPage />} />
            <Route path="employees" element={<EmployeesPage />} />
            <Route path="leave-requests" element={<LeaveRequestsPage />} />
            <Route path="attendance" element={<AttendancePage />} />
            <Route path="documents" element={<DocumentsPage />} />
            <Route path="audit-logs" element={<AuditPage />} />
            <Route path="projects" element={<ProjectListPage />} />
            <Route path="projects/new" element={<ProjectCreatePage />} />
            <Route path="projects/:id" element={<ProjectDetailPage />} />
            <Route path="projects/:id/edit" element={<ProjectEditPage />} />
            <Route path="time-tracking" element={<TimeTrackingPage />} />
            <Route path="time-tracking/manual" element={<ManualEntryPage />} />
            <Route path="time-tracking/calendar" element={<CalendarPage />} />
            <Route path="time-tracking/timesheets" element={<TimesheetsPage />} />
            <Route path="time-tracking/timesheets/approvals" element={<TimesheetApprovalsPage />} />
            <Route path="calendar/team" element={<TeamCalendarPage />} />
            <Route path="calendar/holidays" element={<HolidaysPage />} />
            <Route path="analytics" element={<AnalyticsPage />} />
            <Route path="settings" element={<SettingsPage />} />
            <Route path="settings/geofencing" element={<GeofencingSettingsPage />} />
            <Route path="settings/calendar/callback" element={<CalendarCallbackPage />} />
            <Route element={<PlatformAdminRoute />}>
              <Route path="admin/dashboard" element={<PlatformDashboardPage />} />
              <Route path="admin/tenants/:tenantId" element={<TenantSummaryPage />} />
            </Route>
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
