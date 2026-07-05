import { useState } from 'react';
import { getProjects } from '@/api/projects';
import { Button } from '@/components/ui/button';
import { EmptyState, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { useTimeEntries } from '@/hooks/use-time-tracking';
import { formatDateTime, formatDuration } from '@/lib/utils';
import type { ProjectDto } from '@/types/project';
import type { TimeEntryDto } from '@/types/time-entry';
import { useEffect } from 'react';

const PAGE_SIZE = 20;

interface TimeEntryListProps {
  fromDate?: string;
  toDate?: string;
}

export function TimeEntryList({ fromDate: initialFrom, toDate: initialTo }: TimeEntryListProps) {
  const [page, setPage] = useState(1);
  const [fromDate, setFromDate] = useState(initialFrom ?? '');
  const [toDate, setToDate] = useState(initialTo ?? '');
  const [projectId, setProjectId] = useState('');
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [applied, setApplied] = useState({ fromDate: initialFrom ?? '', toDate: initialTo ?? '', projectId: '' });

  useEffect(() => {
    getProjects({ pageSize: 100, isArchived: false })
      .then((result) => setProjects(result.items))
      .catch(() => setProjects([]));
  }, []);

  const { data, isLoading } = useTimeEntries({
    page,
    pageSize: PAGE_SIZE,
    fromDate: applied.fromDate || undefined,
    toDate: applied.toDate || undefined,
    projectId: applied.projectId || undefined,
  });

  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / PAGE_SIZE));
  const projectName = (id: string) => projects.find((p) => p.id === id)?.name ?? id;

  const applyFilters = () => {
    setPage(1);
    setApplied({ fromDate, toDate, projectId });
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end gap-4">
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">From</label>
          <Input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
        </div>
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">To</label>
          <Input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
        </div>
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">Project</label>
          <Select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
            <option value="">All projects</option>
            {projects.map((project) => (
              <option key={project.id} value={project.id}>
                {project.name}
              </option>
            ))}
          </Select>
        </div>
        <Button type="button" onClick={applyFilters}>
          Apply
        </Button>
      </div>

      {isLoading ? (
        <LoadingSpinner label="Loading time entries" />
      ) : !data?.items.length ? (
        <EmptyState message="No time entries found." />
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase text-muted-foreground">
                  <th className="py-2 pr-4">Start</th>
                  <th className="py-2 pr-4">End</th>
                  <th className="py-2 pr-4">Project</th>
                  <th className="py-2 pr-4">Duration</th>
                  <th className="py-2 pr-4">Billable</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((entry: TimeEntryDto) => (
                  <tr key={entry.id}>
                    <td className="py-2 pr-4 whitespace-nowrap">{formatDateTime(entry.startTime)}</td>
                    <td className="py-2 pr-4 whitespace-nowrap">
                      {entry.endTime ? formatDateTime(entry.endTime) : 'Running'}
                    </td>
                    <td className="py-2 pr-4">{projectName(entry.projectId)}</td>
                    <td className="py-2 pr-4">{formatDuration(entry.workedMinutes)}</td>
                    <td className="py-2 pr-4">{entry.billable ? 'Yes' : 'No'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="flex items-center justify-between">
            <p className="text-xs text-muted-foreground">
              Page {page} of {totalPages} · {data.totalCount} total
            </p>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(page - 1)}>
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage(page + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
