import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { formatDateTime, getApiErrorMessage } from '@/lib/utils';
import { useEmployeesWorking } from '@/hooks/use-analytics';
import type { AnalyticsFilters } from '@/types/analytics';

interface EmployeesWorkingNowProps {
  filters: AnalyticsFilters;
}

export function EmployeesWorkingNow({ filters }: EmployeesWorkingNowProps) {
  const { data, isLoading, isError, error } = useEmployeesWorking(filters);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Employees Working Now</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <LoadingSpinner label="Loading employees" />
        ) : isError ? (
          <ErrorBanner message={getApiErrorMessage(error, 'Failed to load employees working now.')} />
        ) : !data || data.length === 0 ? (
          <EmptyState message="No employees are currently working." />
        ) : (
          <ul className="space-y-3">
            {data.map((employee) => (
              <li key={employee.employeeId} className="flex items-start justify-between gap-2 text-sm">
                <div>
                  <p className="font-medium">{employee.employeeName}</p>
                  {employee.projectName && (
                    <p className="text-xs text-muted-foreground">{employee.projectName}</p>
                  )}
                </div>
                {employee.checkInTime && (
                  <span className="whitespace-nowrap text-xs text-muted-foreground">
                    Since {formatDateTime(employee.checkInTime)}
                  </span>
                )}
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
