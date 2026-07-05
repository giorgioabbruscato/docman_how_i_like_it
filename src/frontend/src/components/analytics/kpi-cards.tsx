import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner } from '@/components/ui/loading-spinner';
import { formatPercent } from '@/lib/utils';
import { useSupervisorSummary } from '@/hooks/use-analytics';
import type { AnalyticsFilters } from '@/types/analytics';

interface KpiCardsProps {
  filters: AnalyticsFilters;
  showBudget?: boolean;
}

export function KpiCards({ filters, showBudget = false }: KpiCardsProps) {
  const { data, isLoading, isError, error } = useSupervisorSummary(filters);

  if (isLoading) {
    return (
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: showBudget ? 4 : 4 }).map((_, index) => (
          <Card key={index}>
            <CardHeader className="pb-2">
              <div className="h-4 w-24 animate-pulse rounded bg-muted" />
            </CardHeader>
            <CardContent>
              <div className="h-8 w-16 animate-pulse rounded bg-muted" />
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  if (isError || !data) {
    return <ErrorBanner message={error?.message ?? 'Failed to load summary metrics.'} />;
  }

  const cards = [
    {
      title: 'Total Hours',
      value: data.totalWorkedHours.toFixed(1),
      subtitle: 'Worked in range',
    },
    {
      title: 'Attendance Rate',
      value: formatPercent(data.attendanceRate, 1),
      subtitle: 'Check-in coverage',
    },
    {
      title: 'Overtime',
      value: String(data.overtime.length),
      subtitle: 'Employees with overtime',
    },
    {
      title: 'Late Arrivals',
      value: String(data.lateArrivals.length),
      subtitle: 'Today',
    },
  ];

  if (showBudget) {
    const overBudget = data.budgetUsage.filter(
      (item) => item.budgetHours != null && item.spentHours > item.budgetHours,
    ).length;
    cards.push({
      title: 'Over Budget',
      value: String(overBudget),
      subtitle: 'Projects exceeding hours',
    });
  }

  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      {cards.map((card) => (
        <Card key={card.title}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {card.title}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{card.value}</p>
            <p className="text-xs text-muted-foreground">{card.subtitle}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
