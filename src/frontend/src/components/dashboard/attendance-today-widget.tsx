import { useQuery } from '@tanstack/react-query';
import { getDashboard } from '@/api/attendance';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import { WidgetSkeleton } from './widget-primitives';

export function AttendanceTodayWidget() {
  const canRead = useHasPermission(Permission.AttendanceSessionReadSelf);
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard-attendance'],
    queryFn: () => getDashboard(),
    enabled: canRead,
  });

  if (!canRead) return null;
  if (isLoading) return <WidgetSkeleton title="Attendance today" />;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Attendance today</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-sm text-muted-foreground">
          {data?.currentSession ? 'Checked in' : 'Not checked in'}
        </p>
        <p className="text-2xl font-bold">{data?.todayWorkedMinutes ?? 0} min</p>
      </CardContent>
    </Card>
  );
}
