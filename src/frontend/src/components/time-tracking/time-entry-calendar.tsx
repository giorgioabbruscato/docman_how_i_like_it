import { useMemo, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, LoadingSpinner } from '@/components/ui/loading-spinner';
import { useTimeEntries } from '@/hooks/use-time-tracking';
import { formatDuration } from '@/lib/utils';
import type { TimeEntryDto } from '@/types/time-entry';

type CalendarView = 'daily' | 'weekly' | 'monthly';

function toDateKey(date: Date): string {
  return date.toISOString().slice(0, 10);
}

function startOfWeek(date: Date): Date {
  const d = new Date(date);
  const day = d.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  d.setDate(d.getDate() + diff);
  d.setHours(0, 0, 0, 0);
  return d;
}

function minutesForDate(entries: TimeEntryDto[], dateKey: string): number {
  return entries
    .filter((entry) => entry.startTime.slice(0, 10) === dateKey)
    .reduce((sum, entry) => sum + entry.workedMinutes, 0);
}

export function TimeEntryCalendar() {
  const [view, setView] = useState<CalendarView>('weekly');
  const [selectedDate, setSelectedDate] = useState(new Date());

  const monthStart = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), 1);
  const monthEnd = new Date(selectedDate.getFullYear(), selectedDate.getMonth() + 1, 0);
  const weekStart = startOfWeek(selectedDate);
  const weekEnd = new Date(weekStart);
  weekEnd.setDate(weekEnd.getDate() + 6);

  const rangeFrom =
    view === 'monthly'
      ? toDateKey(monthStart)
      : view === 'weekly'
        ? toDateKey(weekStart)
        : toDateKey(selectedDate);
  const rangeTo =
    view === 'monthly'
      ? toDateKey(monthEnd)
      : view === 'weekly'
        ? toDateKey(weekEnd)
        : toDateKey(selectedDate);

  const { data, isLoading } = useTimeEntries({
    page: 1,
    pageSize: 500,
    fromDate: rangeFrom,
    toDate: rangeTo,
  });

  const entries = data?.items ?? [];

  const dailyEntries = useMemo(
    () => entries.filter((entry) => entry.startTime.slice(0, 10) === toDateKey(selectedDate)),
    [entries, selectedDate],
  );

  const weekDays = useMemo(() => {
    const days: Date[] = [];
    for (let i = 0; i < 7; i += 1) {
      const day = new Date(weekStart);
      day.setDate(day.getDate() + i);
      days.push(day);
    }
    return days;
  }, [weekStart]);

  const monthGrid = useMemo(() => {
    const firstDay = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), 1);
    const lastDay = new Date(selectedDate.getFullYear(), selectedDate.getMonth() + 1, 0);
    const startOffset = (firstDay.getDay() + 6) % 7;
    const cells: (Date | null)[] = Array.from({ length: startOffset }, () => null);
    for (let day = 1; day <= lastDay.getDate(); day += 1) {
      cells.push(new Date(selectedDate.getFullYear(), selectedDate.getMonth(), day));
    }
    return cells;
  }, [selectedDate]);

  const shiftDate = (days: number) => {
    const next = new Date(selectedDate);
    next.setDate(next.getDate() + days);
    setSelectedDate(next);
  };

  const shiftMonth = (months: number) => {
    const next = new Date(selectedDate);
    next.setMonth(next.getMonth() + months);
    setSelectedDate(next);
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex flex-wrap items-center justify-between gap-4">
          <CardTitle>Calendar</CardTitle>
          <div className="flex gap-2">
            {(['daily', 'weekly', 'monthly'] as CalendarView[]).map((option) => (
              <Button
                key={option}
                size="sm"
                variant={view === option ? 'default' : 'outline'}
                onClick={() => setView(option)}
              >
                {option.charAt(0).toUpperCase() + option.slice(1)}
              </Button>
            ))}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center justify-between">
          <Button variant="outline" size="sm" onClick={() => (view === 'monthly' ? shiftMonth(-1) : shiftDate(view === 'weekly' ? -7 : -1))}>
            Previous
          </Button>
          <p className="text-sm font-medium">
            {selectedDate.toLocaleDateString(undefined, {
              month: 'long',
              year: 'numeric',
              ...(view === 'daily' ? { day: 'numeric' } : {}),
            })}
          </p>
          <Button variant="outline" size="sm" onClick={() => (view === 'monthly' ? shiftMonth(1) : shiftDate(view === 'weekly' ? 7 : 1))}>
            Next
          </Button>
        </div>

        {isLoading ? (
          <LoadingSpinner label="Loading calendar" />
        ) : view === 'daily' ? (
          dailyEntries.length === 0 ? (
            <EmptyState message="No entries for this day." />
          ) : (
            <ul className="divide-y divide-border">
              {dailyEntries.map((entry) => (
                <li key={entry.id} className="py-2 text-sm">
                  <p className="font-medium">{formatDuration(entry.workedMinutes)}</p>
                  <p className="text-muted-foreground">{entry.description ?? 'No description'}</p>
                </li>
              ))}
            </ul>
          )
        ) : view === 'weekly' ? (
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4 lg:grid-cols-7">
            {weekDays.map((day) => {
              const minutes = minutesForDate(entries, toDateKey(day));
              return (
                <div key={day.toISOString()} className="rounded-md border p-3 text-center">
                  <p className="text-xs text-muted-foreground">
                    {day.toLocaleDateString(undefined, { weekday: 'short' })}
                  </p>
                  <p className="text-sm font-medium">{day.getDate()}</p>
                  <p className="mt-2 text-lg font-bold">{formatDuration(minutes)}</p>
                </div>
              );
            })}
          </div>
        ) : (
          <div className="grid grid-cols-7 gap-2 text-center text-xs">
            {['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].map((label) => (
              <div key={label} className="font-medium text-muted-foreground">
                {label}
              </div>
            ))}
            {monthGrid.map((day, index) =>
              day ? (
                <div key={day.toISOString()} className="rounded-md border p-2">
                  <p className="font-medium">{day.getDate()}</p>
                  <p className="text-muted-foreground">{formatDuration(minutesForDate(entries, toDateKey(day)))}</p>
                </div>
              ) : (
                <div key={`empty-${index}`} />
              ),
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
