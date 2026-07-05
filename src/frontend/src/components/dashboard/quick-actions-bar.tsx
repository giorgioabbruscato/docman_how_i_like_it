import { Link } from 'react-router-dom';
import { cn } from '@/lib/utils';
import { Permission, useHasAnyPermission, useHasPermission } from '@/lib/auth-permissions';

const linkClass =
  'inline-flex h-8 items-center justify-center rounded-md border border-border bg-background px-3 text-xs font-medium hover:bg-muted';

export function QuickActionsBar() {
  const canTimesheet = useHasPermission(Permission.TimesheetSubmitSelf);
  const canLeave = useHasPermission(Permission.LeaveCreateSelf);
  const canCheckIn = useHasPermission(Permission.AttendanceSessionCheckInSelf);
  const canCalendar = useHasAnyPermission(Permission.CalendarReadTeam, Permission.CalendarReadSelf);

  if (!canTimesheet && !canLeave && !canCheckIn && !canCalendar) return null;

  return (
    <div className="flex flex-wrap gap-2">
      {canCheckIn && (
        <Link to="/attendance" className={cn(linkClass)}>
          Check in
        </Link>
      )}
      {canLeave && (
        <Link to="/leave-requests" className={cn(linkClass)}>
          Request leave
        </Link>
      )}
      {canTimesheet && (
        <Link to="/time-tracking/timesheets" className={cn(linkClass)}>
          Submit timesheet
        </Link>
      )}
      {canCalendar && (
        <Link to="/calendar/team" className={cn(linkClass)}>
          Team calendar
        </Link>
      )}
    </div>
  );
}
