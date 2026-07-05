import type { AttendanceDashboardDto } from '@/types/attendance';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatDateTime, formatDuration } from '@/lib/utils';

interface TodaySummaryProps {
  dashboard: AttendanceDashboardDto;
}

export function TodaySummary({ dashboard }: TodaySummaryProps) {
  const hasOpenSession = Boolean(dashboard.currentSession && !dashboard.currentSession.checkOut);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Today</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3 text-sm">
        <div className="grid gap-3 sm:grid-cols-2">
          <div>
            <p className="text-muted-foreground">Check-in</p>
            <p className="font-medium">
              {dashboard.todayCheckIn ? formatDateTime(dashboard.todayCheckIn) : '—'}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground">Check-out</p>
            <p className="font-medium">
              {dashboard.todayCheckOut ? formatDateTime(dashboard.todayCheckOut) : '—'}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground">Worked today</p>
            <p className="font-medium">{formatDuration(dashboard.todayWorkedMinutes)}</p>
          </div>
          <div>
            <p className="text-muted-foreground">Status</p>
            <p className="font-medium">{hasOpenSession ? 'Open session' : 'No open session'}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
