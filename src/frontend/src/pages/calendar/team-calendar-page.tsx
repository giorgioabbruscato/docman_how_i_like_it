import { useQuery } from '@tanstack/react-query';
import { fetchCalendarEvents } from '@/api/calendar';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import { Permission, useHasAnyPermission } from '@/lib/auth-permissions';
import { useState } from 'react';

export function TeamCalendarPage() {
  const canRead = useHasAnyPermission(Permission.CalendarReadTeam, Permission.CalendarReadSelf);
  const today = new Date();
  const [fromDate] = useState(
    new Date(today.getFullYear(), today.getMonth(), 1).toISOString().slice(0, 10),
  );
  const [toDate] = useState(
    new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString().slice(0, 10),
  );

  const { data, isLoading } = useQuery({
    queryKey: ['calendar-events', fromDate, toDate],
    queryFn: () => fetchCalendarEvents({ fromDate, toDate }),
    enabled: canRead,
  });

  if (!canRead) {
    return <p className="text-muted-foreground">You do not have permission to view the team calendar.</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Team Calendar</h2>
        <p className="text-muted-foreground">Leave, permissions, holidays, and smart working.</p>
      </div>
      {isLoading && <LoadingSpinner label="Loading calendar" />}
      <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3">
        {data?.map((event) => (
          <Card key={event.id}>
            <CardHeader>
              <CardTitle className="text-sm">{event.title}</CardTitle>
            </CardHeader>
            <CardContent className="text-sm text-muted-foreground">
              <p>
                {event.startDate} — {event.endDate}
              </p>
              <p>{event.type}</p>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
