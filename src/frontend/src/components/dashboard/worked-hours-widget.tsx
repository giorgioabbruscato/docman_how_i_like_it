import { useQuery } from '@tanstack/react-query';
import { getTimeEntries } from '@/api/time-tracking';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Permission, useHasAnyPermission } from '@/lib/auth-permissions';
import { WidgetSkeleton } from './widget-primitives';

export function WorkedHoursWidget() {
  const canRead = useHasAnyPermission(
    Permission.TimeEntryReadSelf,
    Permission.TimeEntryReadTeam,
    Permission.TimeEntryReadTenant,
  );
  const today = new Date().toISOString().slice(0, 10);
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard-hours', today],
    queryFn: () => getTimeEntries({ fromDate: today, toDate: today, pageSize: 100 }),
    enabled: canRead,
  });

  if (!canRead) return null;
  if (isLoading) return <WidgetSkeleton title="Worked hours" />;

  const minutes = data?.items.reduce((sum, e) => sum + e.workedMinutes, 0) ?? 0;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Worked hours today</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-bold">{(minutes / 60).toFixed(1)}h</p>
      </CardContent>
    </Card>
  );
}
