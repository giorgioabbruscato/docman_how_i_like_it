import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { getApiErrorMessage } from '@/lib/utils';
import { useTopEmployees } from '@/hooks/use-analytics';
import type { AnalyticsFilters } from '@/types/analytics';

interface TopEmployeesWidgetProps {
  filters: AnalyticsFilters;
}

export function TopEmployeesWidget({ filters }: TopEmployeesWidgetProps) {
  const { data, isLoading, isError, error } = useTopEmployees(filters);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Top Employees</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <LoadingSpinner label="Loading top employees" />
        ) : isError ? (
          <ErrorBanner message={getApiErrorMessage(error, 'Failed to load top employees.')} />
        ) : !data || data.length === 0 ? (
          <EmptyState message="No employee hours recorded for this range." />
        ) : (
          <ol className="space-y-2">
            {data.map((employee, index) => (
              <li
                key={employee.employeeId}
                className="flex items-center justify-between gap-2 text-sm"
              >
                <span>
                  <span className="mr-2 text-muted-foreground">{index + 1}.</span>
                  {employee.employeeName}
                </span>
                <span className="font-medium">{employee.hours.toFixed(1)}h</span>
              </li>
            ))}
          </ol>
        )}
      </CardContent>
    </Card>
  );
}
