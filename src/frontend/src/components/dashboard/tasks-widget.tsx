import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getTasks } from '@/api/tasks';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Permission, useHasAnyPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';
import { WidgetSkeleton } from './widget-primitives';

const statusStyles: Record<string, string> = {
  Todo: 'bg-muted text-muted-foreground',
  InProgress: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
  Review: 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200',
  Done: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
};

export function TasksWidget() {
  const employeeId = useAuthStore((state) => state.employeeId);
  const canRead = useHasAnyPermission(Permission.TaskReadSelf, Permission.TaskReadTenant);
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard-tasks', employeeId],
    queryFn: () =>
      getTasks({
        assignedEmployeeId: employeeId ?? undefined,
        pageSize: 5,
      }),
    enabled: canRead && !!employeeId,
  });

  if (!canRead) return null;
  if (isLoading) return <WidgetSkeleton title="Assigned tasks" />;

  const tasks = data?.items ?? [];

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-base">Assigned tasks</CardTitle>
        <Link to="/projects" className="text-sm text-primary hover:underline">
          View projects
        </Link>
      </CardHeader>
      <CardContent>
        {tasks.length === 0 ? (
          <p className="text-sm text-muted-foreground">No assigned tasks.</p>
        ) : (
          <ul className="space-y-2">
            {tasks.map((task) => (
              <li key={task.id} className="flex items-center justify-between gap-2 text-sm">
                <span className="truncate font-medium">{task.title}</span>
                <span
                  className={`shrink-0 rounded px-2 py-0.5 text-xs font-medium ${statusStyles[task.status] ?? statusStyles.Todo}`}
                >
                  {task.status}
                </span>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
