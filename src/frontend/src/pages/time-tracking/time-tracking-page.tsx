import { Link } from 'react-router-dom';
import { ExportButton } from '@/components/time-tracking/export-button';
import { TimeEntryList } from '@/components/time-tracking/time-entry-list';
import { TimerWidget } from '@/components/time-tracking/timer-widget';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';

export function TimeTrackingPage() {
  const permissions = useAuthStore((state) => state.permissions);
  const canRead = hasAnyPermission(
    permissions,
    Permission.TimeEntryReadSelf,
    Permission.TimeEntryReadTeam,
    Permission.TimeEntryReadTenant,
  );

  if (!canRead) {
    return (
      <div className="space-y-6">
        <h2 className="text-3xl font-bold tracking-tight">Time Tracking</h2>
        <p className="text-muted-foreground">You do not have permission to view time entries.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Time Tracking</h2>
          <p className="text-muted-foreground">Track time with the timer or manual entries.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link to="/time-tracking/manual">
            <Button type="button" variant="outline">Manual Entry</Button>
          </Link>
          <Link to="/time-tracking/calendar">
            <Button type="button" variant="outline">Calendar</Button>
          </Link>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <TimerWidget />
        <ExportButton />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Recent Entries</CardTitle>
        </CardHeader>
        <CardContent>
          <TimeEntryList />
        </CardContent>
      </Card>
    </div>
  );
}
