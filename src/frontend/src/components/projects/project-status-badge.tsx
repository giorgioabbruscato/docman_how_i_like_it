import { cn } from '@/lib/utils';
import type { ProjectStatus } from '@/types/project';

const statusStyles: Record<ProjectStatus, string> = {
  Active: 'bg-green-100 text-green-800',
  OnHold: 'bg-yellow-100 text-yellow-800',
  Completed: 'bg-blue-100 text-blue-800',
  Cancelled: 'bg-red-100 text-red-800',
};

export function ProjectStatusBadge({ status }: { status: ProjectStatus }) {
  return (
    <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', statusStyles[status])}>
      {status}
    </span>
  );
}
