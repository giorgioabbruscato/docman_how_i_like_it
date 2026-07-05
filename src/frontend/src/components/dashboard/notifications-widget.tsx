import { useQuery } from '@tanstack/react-query';
import { fetchNotifications } from '@/api/notifications';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { WidgetSkeleton } from './widget-primitives';

export function NotificationsWidget() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['notifications'],
    queryFn: () => fetchNotifications(1, 5),
  });

  if (isLoading) return <WidgetSkeleton title="Notifications" />;
  if (isError) return null;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Notifications</CardTitle>
      </CardHeader>
      <CardContent className="space-y-2">
        {data?.items.length === 0 && (
          <p className="text-sm text-muted-foreground">No notifications yet.</p>
        )}
        {data?.items.map((n) => (
          <div key={n.id} className="text-sm">
            <p className="font-medium">{n.title}</p>
            <p className="text-muted-foreground">{n.body}</p>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}
