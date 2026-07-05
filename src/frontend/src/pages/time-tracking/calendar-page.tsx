import { Link } from 'react-router-dom';
import { TimeEntryCalendar } from '@/components/time-tracking/time-entry-calendar';
import { Button } from '@/components/ui/button';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';

export function CalendarPage() {
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
        <h2 className="text-3xl font-bold tracking-tight">Calendar</h2>
        <p className="text-muted-foreground">You do not have permission to view time entries.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Time Calendar</h2>
          <p className="text-muted-foreground">Daily, weekly, and monthly views of your time.</p>
        </div>
        <Link to="/time-tracking">
          <Button type="button" variant="outline">Back</Button>
        </Link>
      </div>
      <TimeEntryCalendar />
    </div>
  );
}
