import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from '@/components/auth/protected-route';
import { AppLayout } from '@/components/layout/app-layout';
import { AttendancePage } from '@/pages/attendance-page';
import { DashboardPage } from '@/pages/dashboard-page';
import { DepartmentsPage } from '@/pages/departments-page';
import { DocumentsPage } from '@/pages/documents-page';
import { EmployeesPage } from '@/pages/employees-page';
import { LeaveRequestsPage } from '@/pages/leave-requests-page';
import { LoginPage } from '@/pages/login-page';
import { SettingsPage } from '@/pages/settings-page';

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
            <Route path="settings" element={<SettingsPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
