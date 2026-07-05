import { useQuery } from '@tanstack/react-query';
import { fetchLeaveRequests } from '@/api/leave-requests';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import { WidgetSkeleton } from './widget-primitives';

export function LeaveBalanceWidget() {
  const canRead = useHasPermission(Permission.LeaveReadSelf);
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard-leave'],
    queryFn: fetchLeaveRequests,
    enabled: canRead,
  });

  if (!canRead) return null;
  if (isLoading) return <WidgetSkeleton title="Leave balance" />;

  const approvedDays =
    data
      ?.filter((r) => r.status === 'Approved' && r.type === 'Annual')
      .reduce((sum, r) => {
        const start = new Date(r.startDate);
        const end = new Date(r.endDate);
        return sum + Math.round((end.getTime() - start.getTime()) / 86400000) + 1;
      }, 0) ?? 0;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Annual leave used</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-bold">{approvedDays} days</p>
      </CardContent>
    </Card>
  );
}
