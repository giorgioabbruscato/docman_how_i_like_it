import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import { useCreateTimesheet, useSubmitTimesheet, useTimesheets } from '@/hooks/use-timesheets';
import { Permission, useHasAnyPermission } from '@/lib/auth-permissions';
import type { TimesheetSubmission } from '@/types/timesheet';

function formatMinutes(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return `${hours}h ${mins}m`;
}

export function TimesheetsPage() {
  const canSubmit = useHasAnyPermission(Permission.TimesheetSubmitSelf);
  const canRead = useHasAnyPermission(Permission.TimesheetReadSelf, Permission.TimesheetReadTeam);

  const { data, isLoading } = useTimesheets({ status: undefined });
  const createMutation = useCreateTimesheet();
  const submitMutation = useSubmitTimesheet();

  const [periodStart, setPeriodStart] = useState('');
  const [periodEnd, setPeriodEnd] = useState('');
  const [notes, setNotes] = useState('');

  const handleCreate = async () => {
    if (!periodStart || !periodEnd) return;
    await createMutation.mutateAsync({ periodStart, periodEnd, notes: notes || null });
    setNotes('');
  };

  const handleSubmit = async (id: string) => {
    if (!confirm('Submit this timesheet for approval?')) return;
    await submitMutation.mutateAsync(id);
  };

  if (!canRead) {
    return <p className="text-muted-foreground">You do not have permission to view timesheets.</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">My Timesheets</h2>
        <p className="text-muted-foreground">Create and submit timesheets for supervisor approval.</p>
      </div>

      {canSubmit && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">New timesheet</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label htmlFor="periodStart" className="text-sm font-medium">Period start</label>
              <Input
                id="periodStart"
                type="date"
                value={periodStart}
                onChange={(e) => setPeriodStart(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="periodEnd" className="text-sm font-medium">Period end</label>
              <Input
                id="periodEnd"
                type="date"
                value={periodEnd}
                onChange={(e) => setPeriodEnd(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="notes" className="text-sm font-medium">Notes</label>
              <Input
                id="notes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Optional notes"
              />
            </div>
            <div className="md:col-span-3">
              <Button
                onClick={() => void handleCreate()}
                disabled={createMutation.isPending || !periodStart || !periodEnd}
              >
                Create draft
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {isLoading && <LoadingSpinner label="Loading timesheets" />}

      <div className="grid gap-4">
        {data?.items.map((timesheet: TimesheetSubmission) => (
          <Card key={timesheet.id}>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-base">
                {timesheet.periodStart} — {timesheet.periodEnd}
              </CardTitle>
              <span className="rounded-full bg-muted px-2 py-1 text-xs">{timesheet.status}</span>
            </CardHeader>
            <CardContent className="space-y-2">
              <p className="text-sm text-muted-foreground">
                {formatMinutes(timesheet.totalWorkedMinutes)} · {timesheet.timeEntryIds.length} entries
              </p>
              {timesheet.notes && <p className="text-sm">{timesheet.notes}</p>}
              {canSubmit && timesheet.status === 'Draft' && (
                <Button
                  size="sm"
                  onClick={() => void handleSubmit(timesheet.id)}
                  disabled={submitMutation.isPending}
                >
                  Submit for approval
                </Button>
              )}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
