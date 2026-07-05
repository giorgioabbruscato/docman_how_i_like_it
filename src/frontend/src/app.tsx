import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from '@/components/auth/protected-route';
import { AppLayout } from '@/components/layout/app-layout';
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

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route index element={<DashboardPage />} />
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
            <Route path="settings" element={<SettingsPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
