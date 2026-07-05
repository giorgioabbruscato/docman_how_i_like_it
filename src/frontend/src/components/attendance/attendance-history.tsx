import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { useAttendanceHistory } from '@/hooks/use-attendance';
import { formatDateTime, formatDuration, getApiErrorMessage } from '@/lib/utils';

const PAGE_SIZE = 10;

export function AttendanceHistory() {
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [applied, setApplied] = useState({ fromDate: '', toDate: '', page: 1 });

  const { data, isLoading, error } = useAttendanceHistory({
    page: applied.page,
    pageSize: PAGE_SIZE,
    fromDate: applied.fromDate || undefined,
    toDate: applied.toDate || undefined,
  });

  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / PAGE_SIZE));

  const applyFilters = () => {
    setApplied({ fromDate, toDate, page: 1 });
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
              {data.items.map((session) => (
                <li key={session.id} className="py-3 text-sm">
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
                </li>
              ))}
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
