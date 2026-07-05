import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { getApiErrorMessage } from '@/lib/utils';
import { useTopProjects } from '@/hooks/use-analytics';
import type { AnalyticsFilters } from '@/types/analytics';

interface TopProjectsWidgetProps {
  filters: AnalyticsFilters;
}

export function TopProjectsWidget({ filters }: TopProjectsWidgetProps) {
  const { data, isLoading, isError, error } = useTopProjects(filters);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Top Projects</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <LoadingSpinner label="Loading top projects" />
        ) : isError ? (
          <ErrorBanner message={getApiErrorMessage(error, 'Failed to load top projects.')} />
        ) : !data || data.length === 0 ? (
          <EmptyState message="No project hours recorded for this range." />
        ) : (
          <ol className="space-y-2">
            {data.map((project, index) => (
              <li
                key={project.projectId}
                className="flex items-center justify-between gap-2 text-sm"
              >
                <span>
                  <span className="mr-2 text-muted-foreground">{index + 1}.</span>
                  {project.projectName}
                </span>
                <span className="font-medium">{project.hours.toFixed(1)}h</span>
              </li>
            ))}
          </ol>
        )}
      </CardContent>
    </Card>
  );
}
