import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import {
  useApproveTimesheet,
  useRejectTimesheet,
  useTimesheets,
} from '@/hooks/use-timesheets';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import type { TimesheetSubmission } from '@/types/timesheet';

function formatMinutes(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return `${hours}h ${mins}m`;
}

export function TimesheetApprovalsPage() {
  const canApprove = useHasPermission(Permission.TimesheetApproveTeam);
  const { data, isLoading } = useTimesheets({ status: 'Submitted' });
  const approveMutation = useApproveTimesheet();
  const rejectMutation = useRejectTimesheet();
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [rejectComment, setRejectComment] = useState('');

  if (!canApprove) {
    return <p className="text-muted-foreground">You do not have permission to approve timesheets.</p>;
  }

  const handleApprove = async (id: string) => {
    if (!confirm('Approve this timesheet?')) return;
    await approveMutation.mutateAsync(id);
  };

  const handleReject = async () => {
    if (!rejectId) return;
    await rejectMutation.mutateAsync({ id: rejectId, input: { comment: rejectComment || null } });
    setRejectId(null);
    setRejectComment('');
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Timesheet Approvals</h2>
        <p className="text-muted-foreground">Review and approve submitted timesheets.</p>
      </div>

      {isLoading && <LoadingSpinner label="Loading pending timesheets" />}

      <div className="grid gap-4">
        {data?.items.length === 0 && !isLoading && (
          <p className="text-muted-foreground">No timesheets pending approval.</p>
        )}
        {data?.items.map((timesheet: TimesheetSubmission) => (
          <Card key={timesheet.id}>
            <CardHeader>
              <CardTitle className="text-base">
                {timesheet.periodStart} — {timesheet.periodEnd}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <p className="text-sm text-muted-foreground">
                Employee {timesheet.employeeId.slice(0, 8)}… ·{' '}
                {formatMinutes(timesheet.totalWorkedMinutes)}
              </p>
              <div className="flex gap-2">
                <Button
                  size="sm"
                  onClick={() => void handleApprove(timesheet.id)}
                  disabled={approveMutation.isPending}
                >
                  Approve
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => setRejectId(timesheet.id)}
                >
                  Reject
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {rejectId && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Reject timesheet</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="space-y-2">
              <label htmlFor="rejectComment" className="text-sm font-medium">Comment</label>
              <Input
                id="rejectComment"
                value={rejectComment}
                onChange={(e) => setRejectComment(e.target.value)}
                placeholder="Reason for rejection"
              />
            </div>
            <div className="flex gap-2">
              <Button
                variant="destructive"
                size="sm"
                onClick={() => void handleReject()}
                disabled={rejectMutation.isPending}
              >
                Confirm reject
              </Button>
              <Button size="sm" variant="ghost" onClick={() => setRejectId(null)}>
                Cancel
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
