import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { EmptyState, LoadingSpinner } from '@/components/ui/loading-spinner';
import { formatDate } from '@/lib/utils';
import type { ProjectDto } from '@/types/project';
import { ProjectStatusBadge } from './project-status-badge';

interface ProjectListProps {
  projects: ProjectDto[];
  loading: boolean;
  page: number;
  totalPages: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

export function ProjectList({
  projects,
  loading,
  page,
  totalPages,
  totalCount,
  onPageChange,
}: ProjectListProps) {
  if (loading) {
    return <LoadingSpinner label="Loading projects" />;
  }

  if (projects.length === 0) {
    return <EmptyState message="No projects found." />;
  }

  return (
    <>
      <div className="hidden overflow-x-auto md:block">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border text-left text-xs uppercase text-muted-foreground">
              <th className="py-2 pr-4">Name</th>
              <th className="py-2 pr-4">Customer</th>
              <th className="py-2 pr-4">Status</th>
              <th className="py-2 pr-4">Start</th>
              <th className="py-2 pr-4">End</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {projects.map((project) => (
              <tr key={project.id} className="hover:bg-muted/50">
                <td className="py-2 pr-4">
                  <Link to={`/projects/${project.id}`} className="font-medium hover:underline">
                    {project.name}
                  </Link>
                  {project.isArchived && (
                    <span className="ml-2 text-xs text-muted-foreground">(archived)</span>
                  )}
                </td>
                <td className="py-2 pr-4">{project.customerName ?? '—'}</td>
                <td className="py-2 pr-4">
                  <ProjectStatusBadge status={project.status} />
                </td>
                <td className="py-2 pr-4">
                  {project.startDate ? formatDate(project.startDate) : '—'}
                </td>
                <td className="py-2 pr-4">
                  {project.endDate ? formatDate(project.endDate) : '—'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="space-y-3 md:hidden">
        {projects.map((project) => (
          <Link
            key={project.id}
            to={`/projects/${project.id}`}
            className="block rounded-md border border-border p-4 hover:bg-muted/50"
          >
            <div className="flex items-start justify-between gap-2">
              <p className="font-medium">{project.name}</p>
              <ProjectStatusBadge status={project.status} />
            </div>
            <p className="mt-1 text-sm text-muted-foreground">
              {project.customerName ?? 'No customer'}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              {project.startDate ? formatDate(project.startDate) : '—'} →{' '}
              {project.endDate ? formatDate(project.endDate) : '—'}
            </p>
          </Link>
        ))}
      </div>

      <div className="mt-4 flex items-center justify-between">
        <p className="text-xs text-muted-foreground">
          Page {page} of {totalPages} · {totalCount} total
        </p>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            disabled={page <= 1}
            onClick={() => onPageChange(page - 1)}
          >
            Previous
          </Button>
          <Button
            variant="outline"
            size="sm"
            disabled={page >= totalPages}
            onClick={() => onPageChange(page + 1)}
          >
            Next
          </Button>
        </div>
      </div>
    </>
  );
}
