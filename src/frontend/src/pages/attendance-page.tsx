import { useEffect, useState } from 'react';
import { AttendanceHistory } from '@/components/attendance/attendance-history';
import { CheckInButton } from '@/components/attendance/check-in-button';
import { CheckOutButton } from '@/components/attendance/check-out-button';
import { GpsStatus } from '@/components/attendance/gps-status';
import { LiveTimer } from '@/components/attendance/live-timer';
import { TodaySummary } from '@/components/attendance/today-summary';
import { WeeklyMonthlyTotals } from '@/components/attendance/weekly-monthly-totals';
import { ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { SuccessBanner } from '@/components/ui/success-banner';
import { useAttendanceDashboard } from '@/hooks/use-attendance';
import { Permission, hasAnyPermission, hasPermission } from '@/lib/auth-permissions';
import { getApiErrorMessage } from '@/lib/utils';
import { useAttendanceStore } from '@/stores/attendance-store';
import { useAuthStore } from '@/stores/auth-store';

export function AttendancePage() {
  const permissions = useAuthStore((state) => state.permissions);
  const canRead = hasAnyPermission(
    permissions,
    Permission.AttendanceSessionReadSelf,
    Permission.AttendanceSessionReadTeam,
    Permission.AttendanceSessionReadTenant,
  );
  const canCheckIn = hasPermission(permissions, Permission.AttendanceSessionCheckInSelf);
  const canCheckOut = hasPermission(permissions, Permission.AttendanceSessionCheckOutSelf);

  const { gpsStatus, successMessage, setSuccessMessage, probeGps } = useAttendanceStore();
  const { data: dashboard, isLoading, error } = useAttendanceDashboard();
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    void probeGps();
  }, [probeGps]);

  if (!canRead) {
    return (
      <div className="space-y-6">
        <h2 className="text-3xl font-bold tracking-tight">Attendance</h2>
        <p className="text-muted-foreground">You do not have permission to view attendance.</p>
      </div>
    );
  }

  const currentSession = dashboard?.currentSession;
  const hasOpenSession = Boolean(currentSession && !currentSession.checkOut);

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Attendance</h2>
        <p className="text-muted-foreground">Check in and out with one tap.</p>
      </div>

      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={getApiErrorMessage(error, 'Failed to load attendance dashboard.')} />}
      {successMessage && (
        <SuccessBanner message={successMessage} onDismiss={() => setSuccessMessage(null)} />
      )}

      <GpsStatus status={gpsStatus} />
      {gpsStatus === 'denied' && (
        <p className="text-sm text-yellow-700">
          Location access is denied. You can still check in/out, but coordinates will not be recorded.
        </p>
      )}

      {isLoading ? (
        <LoadingSpinner label="Loading attendance" />
      ) : dashboard ? (
        <>
          <TodaySummary dashboard={dashboard} />

          {hasOpenSession && currentSession && <LiveTimer session={currentSession} />}

          {canCheckIn && !hasOpenSession && (
            <CheckInButton
              onSuccess={() => setActionError(null)}
              onError={(message) => setActionError(message)}
            />
          )}

          {canCheckOut && hasOpenSession && (
            <CheckOutButton
              onSuccess={() => setActionError(null)}
              onError={(message) => setActionError(message)}
            />
          )}

          <WeeklyMonthlyTotals dashboard={dashboard} />
        </>
      ) : null}

      <AttendanceHistory />
    </div>
  );
}
