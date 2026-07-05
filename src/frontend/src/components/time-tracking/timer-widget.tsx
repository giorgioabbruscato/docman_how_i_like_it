import { useEffect, useState } from 'react';
import { ProjectTaskSelector } from '@/components/time-tracking/project-task-selector';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { useActiveTimer, useStartTimer, useStopTimer } from '@/hooks/use-time-tracking';
import { getApiErrorMessage, formatElapsed } from '@/lib/utils';
import { getElapsedSeconds, useTimerStore } from '@/stores/timer-store';

export function TimerWidget() {
  const { syncActiveTimer, activeEntry, setActiveEntry } = useTimerStore();
  const { data: serverEntry } = useActiveTimer();
  const startTimer = useStartTimer();
  const stopTimer = useStopTimer();

  const [projectId, setProjectId] = useState('');
  const [taskId, setTaskId] = useState('');
  const [description, setDescription] = useState('');
  const [billable, setBillable] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [elapsedSeconds, setElapsedSeconds] = useState(0);

  const entry = activeEntry ?? serverEntry ?? null;
  const isRunning = Boolean(entry && !entry.endTime);

  useEffect(() => {
    void syncActiveTimer();
  }, [syncActiveTimer]);

  useEffect(() => {
    if (entry) {
      setProjectId(entry.projectId);
      setTaskId(entry.taskId ?? '');
      setDescription(entry.description ?? '');
      setBillable(entry.billable);
    }
  }, [entry]);

  useEffect(() => {
    if (!isRunning || !entry) {
      setElapsedSeconds(getElapsedSeconds(entry));
      return;
    }

    const tick = () => setElapsedSeconds(getElapsedSeconds(entry));
    tick();
    const interval = window.setInterval(tick, 1000);
    return () => window.clearInterval(interval);
  }, [isRunning, entry]);

  const handleStart = async () => {
    if (!projectId) {
      setError('Please select a project.');
      return;
    }
    try {
      setError(null);
      const started = await startTimer.mutateAsync({
        projectId,
        taskId: taskId || undefined,
        description: description || undefined,
        billable,
      });
      setActiveEntry(started);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to start timer.'));
    }
  };

  const handleStop = async () => {
    try {
      setError(null);
      await stopTimer.mutateAsync();
      setActiveEntry(null);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to stop timer.'));
    }
  };

  const isPending = startTimer.isPending || stopTimer.isPending;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Timer</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {error && <ErrorBanner message={error} />}

        <div className="text-center">
          <p className="font-mono text-4xl font-bold tracking-wider">{formatElapsed(elapsedSeconds)}</p>
          <p className="text-sm text-muted-foreground">
            {isRunning ? 'Timer running' : 'Timer stopped'}
          </p>
        </div>

        <ProjectTaskSelector
          projectId={projectId}
          taskId={taskId}
          onProjectChange={setProjectId}
          onTaskChange={setTaskId}
          disabled={isRunning || isPending}
        />

        <Input
          placeholder="Description (optional)"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          disabled={isRunning || isPending}
        />

        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={billable}
            onChange={(e) => setBillable(e.target.checked)}
            disabled={isRunning || isPending}
            className="h-4 w-4 rounded border-border"
          />
          Billable
        </label>

        <div className="flex gap-2">
          {isRunning ? (
            <Button
              variant="destructive"
              className="flex-1"
              onClick={() => void handleStop()}
              disabled={isPending}
            >
              {stopTimer.isPending ? 'Stopping...' : 'Stop Timer'}
            </Button>
          ) : (
            <Button className="flex-1" onClick={() => void handleStart()} disabled={isPending}>
              {startTimer.isPending ? 'Starting...' : 'Start Timer'}
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
