import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import type { PlatformDashboardSummary } from '@/types/platform-admin';

interface PlatformKpiCardsProps {
  summary: PlatformDashboardSummary;
}

function formatLicenseUtilization(used: number, total: number): string {
  if (total <= 0) return '—';
  return `${used.toLocaleString()} / ${total.toLocaleString()} (${Math.round((used / total) * 100)}%)`;
}

export function PlatformKpiCards({ summary }: PlatformKpiCardsProps) {
  const cards = [
    {
      title: 'Active Tenants',
      value: summary.totalTenants.toLocaleString(),
      subtitle: 'Currently active on the platform',
    },
    {
      title: 'Total Employees',
      value: summary.totalEmployees.toLocaleString(),
      subtitle: 'Across all tenants',
    },
    {
      title: 'Active Users (30d)',
      value: summary.activeEmployeesLast30Days.toLocaleString(),
      subtitle: 'Employees with recent activity',
    },
    {
      title: 'License Utilization',
      value: formatLicenseUtilization(summary.licenseSeatsUsed, summary.licenseSeatsTotal),
      subtitle: `${summary.totalTimeEntriesLast30Days.toLocaleString()} time entries in last 30 days`,
    },
  ];

  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      {cards.map((card) => (
        <Card key={card.title}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">{card.title}</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{card.value}</p>
            <p className="text-xs text-muted-foreground">{card.subtitle}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
