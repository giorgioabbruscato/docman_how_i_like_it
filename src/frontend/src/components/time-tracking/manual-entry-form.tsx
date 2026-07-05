import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { ProjectTaskSelector } from '@/components/time-tracking/project-task-selector';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { useCreateManualEntry } from '@/hooks/use-time-tracking';
import { getApiErrorMessage, todayDateString } from '@/lib/utils';
import { useState } from 'react';

const manualEntrySchema = z.object({
  date: z.string().min(1, 'Required'),
  hours: z.string().min(1, 'Required'),
  description: z.string().optional(),
  billable: z.boolean(),
});

type ManualEntryFormValues = z.infer<typeof manualEntrySchema>;

export function ManualEntryForm() {
  const navigate = useNavigate();
  const createEntry = useCreateManualEntry();
  const [projectId, setProjectId] = useState('');
  const [taskId, setTaskId] = useState('');
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ManualEntryFormValues>({
    resolver: zodResolver(manualEntrySchema),
    defaultValues: {
      date: todayDateString(),
      billable: true,
    },
  });

  const onSubmit = async (data: ManualEntryFormValues) => {
    if (!projectId) {
      setError('Please select a project.');
      return;
    }
    try {
      setError(null);
      await createEntry.mutateAsync({
        date: data.date,
        projectId,
        taskId: taskId || undefined,
        hours: Number(data.hours),
        description: data.description || undefined,
        billable: data.billable,
      });
      navigate('/time-tracking');
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to create manual entry.'));
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Manual Time Entry</CardTitle>
      </CardHeader>
      <CardContent>
        {error && <ErrorBanner message={error} />}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Date</label>
            <Input type="date" {...register('date')} />
            {errors.date && <p className="mt-1 text-xs text-red-600">{errors.date.message}</p>}
          </div>

          <ProjectTaskSelector
            projectId={projectId}
            taskId={taskId}
            onProjectChange={setProjectId}
            onTaskChange={setTaskId}
          />

          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Hours</label>
            <Input type="number" step="0.25" min="0.25" placeholder="2.5" {...register('hours')} />
            {errors.hours && <p className="mt-1 text-xs text-red-600">{errors.hours.message}</p>}
          </div>

          <Input placeholder="Description (optional)" {...register('description')} />

          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" {...register('billable')} className="h-4 w-4 rounded border-border" />
            Billable
          </label>

          <div className="flex gap-2">
            <Button type="submit" disabled={isSubmitting || createEntry.isPending}>
              {isSubmitting ? 'Saving...' : 'Save Entry'}
            </Button>
            <Button type="button" variant="outline" onClick={() => navigate('/time-tracking')}>
              Cancel
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
