import type { AttendanceDashboardDto } from '@/types/attendance';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatDuration } from '@/lib/utils';

interface WeeklyMonthlyTotalsProps {
  dashboard: AttendanceDashboardDto;
}

export function WeeklyMonthlyTotals({ dashboard }: WeeklyMonthlyTotalsProps) {
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>This Week</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-3xl font-bold">{formatDuration(dashboard.weeklyTotalMinutes)}</p>
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>This Month</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-3xl font-bold">{formatDuration(dashboard.monthlyTotalMinutes)}</p>
        </CardContent>
      </Card>
    </div>
  );
}
