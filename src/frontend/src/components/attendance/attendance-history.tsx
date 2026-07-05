import { useQuery } from '@tanstack/react-query';
import { ChevronDown } from 'lucide-react';
import { useState } from 'react';
import { fetchGeofenceZones, type GeofenceZone } from '@/api/geofence';
import { AttendanceLocationMapLazy } from '@/components/attendance/attendance-location-map-lazy';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { useAttendanceHistory } from '@/hooks/use-attendance';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import { cn, formatDateTime, formatDuration, getApiErrorMessage } from '@/lib/utils';
import type { AttendanceSessionDto } from '@/types/attendance';

const PAGE_SIZE = 10;

function SessionDetailPanel({
  session,
  geofenceZones,
}: {
  session: AttendanceSessionDto;
  geofenceZones?: GeofenceZone[];
}) {
  const deviceLabel = [session.device, session.browser].filter(Boolean).join(' · ') || 'Not recorded';

  return (
    <div className="mt-3 space-y-3 rounded-md border border-border bg-muted/20 p-3">
      <div className="grid gap-2 text-xs sm:grid-cols-2">
        <div>
          <p className="font-medium text-muted-foreground">Device / browser</p>
          <p>{deviceLabel}</p>
        </div>
        {session.accuracyCheckIn != null && (
          <div>
            <p className="font-medium text-muted-foreground">Check-in accuracy</p>
            <p>±{Math.round(session.accuracyCheckIn)} m</p>
          </div>
        )}
        {session.accuracyCheckOut != null && (
          <div>
            <p className="font-medium text-muted-foreground">Check-out accuracy</p>
            <p>±{Math.round(session.accuracyCheckOut)} m</p>
          </div>
        )}
      </div>

      <AttendanceLocationMapLazy
        checkInLat={session.latitudeCheckIn}
        checkInLng={session.longitudeCheckIn}
        checkOutLat={session.latitudeCheckOut}
        checkOutLng={session.longitudeCheckOut}
        checkInAt={session.checkIn}
        checkOutAt={session.checkOut}
        geofenceZones={geofenceZones}
      />
    </div>
  );
}

export function AttendanceHistory() {
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [applied, setApplied] = useState({ fromDate: '', toDate: '', page: 1 });
  const [expandedSessionId, setExpandedSessionId] = useState<string | null>(null);

  const canReadGeofence = useHasPermission(Permission.GeofenceReadTenant);
  const { data: geofenceZones } = useQuery({
    queryKey: ['geofence-zones'],
    queryFn: fetchGeofenceZones,
    enabled: canReadGeofence,
  });

  const { data, isLoading, error } = useAttendanceHistory({
    page: applied.page,
    pageSize: PAGE_SIZE,
    fromDate: applied.fromDate || undefined,
    toDate: applied.toDate || undefined,
  });

  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / PAGE_SIZE));

  const applyFilters = () => {
    setApplied({ fromDate, toDate, page: 1 });
    setExpandedSessionId(null);
  };

  const toggleSession = (sessionId: string) => {
    setExpandedSessionId((current) => (current === sessionId ? null : sessionId));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Recent Sessions</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex flex-wrap items-end gap-3">
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">From</label>
            <Input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">To</label>
            <Input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
          </div>
          <Button type="button" onClick={applyFilters}>
            Apply
          </Button>
        </div>

        {error && <ErrorBanner message={getApiErrorMessage(error, 'Failed to load attendance history.')} />}

        {isLoading ? (
          <LoadingSpinner label="Loading history" />
        ) : !data?.items.length ? (
          <EmptyState message="No attendance sessions found." />
        ) : (
          <>
            <ul className="divide-y divide-border">
              {data.items.map((session) => {
                const isExpanded = expandedSessionId === session.id;

                return (
                  <li key={session.id} className="py-3 text-sm">
                    <button
                      type="button"
                      className="flex w-full items-start justify-between gap-3 text-left"
                      onClick={() => toggleSession(session.id)}
                      aria-expanded={isExpanded}
                    >
                      <div>
                        <p className="font-medium">
                          {formatDateTime(session.checkIn)}
                          {session.checkOut ? ` → ${formatDateTime(session.checkOut)}` : ' → Open'}
                        </p>
                        <p className="text-muted-foreground">
                          {session.workedMinutes != null
                            ? formatDuration(session.workedMinutes)
                            : 'In progress'}{' '}
                          · {session.status}
                        </p>
                      </div>
                      <ChevronDown
                        className={cn(
                          'mt-0.5 h-4 w-4 shrink-0 text-muted-foreground transition-transform',
                          isExpanded && 'rotate-180',
                        )}
                        aria-hidden
                      />
                    </button>

                    {isExpanded && (
                      <SessionDetailPanel
                        session={session}
                        geofenceZones={canReadGeofence ? geofenceZones : undefined}
                      />
                    )}
                  </li>
                );
              })}
            </ul>

            <div className="flex items-center justify-between">
              <p className="text-xs text-muted-foreground">
                Page {applied.page} of {totalPages} · {data.totalCount} total
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={applied.page <= 1}
                  onClick={() => {
                    setApplied((prev) => ({ ...prev, page: prev.page - 1 }));
                    setExpandedSessionId(null);
                  }}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={applied.page >= totalPages}
                  onClick={() => {
                    setApplied((prev) => ({ ...prev, page: prev.page + 1 }));
                    setExpandedSessionId(null);
                  }}
                >
                  Next
                </Button>
              </div>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}
