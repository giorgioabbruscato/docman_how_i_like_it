import type { ReactNode } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner } from '@/components/ui/loading-spinner';

interface ChartCardProps {
  title: string;
  isLoading: boolean;
  isError: boolean;
  errorMessage?: string;
  isEmpty: boolean;
  emptyMessage?: string;
  children: ReactNode;
  className?: string;
}

export function ChartCard({
  title,
  isLoading,
  isError,
  errorMessage = 'Failed to load chart data.',
  isEmpty,
  emptyMessage = 'No data for the selected filters.',
  children,
  className,
}: ChartCardProps) {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="text-lg">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="h-64 animate-pulse rounded-md bg-muted" aria-label="Loading chart" />
        ) : isError ? (
          <ErrorBanner message={errorMessage} />
        ) : isEmpty ? (
          <EmptyState message={emptyMessage} />
        ) : (
          children
        )}
      </CardContent>
    </Card>
  );
}
