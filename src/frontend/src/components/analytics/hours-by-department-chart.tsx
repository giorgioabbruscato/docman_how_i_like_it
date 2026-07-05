import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { ChartCard } from '@/components/analytics/chart-card';
import { CHART_COLORS, chartDatasetKeys, chartResponseToRechartsData } from '@/lib/chart-utils';
import { getApiErrorMessage } from '@/lib/utils';
import { useHoursByDepartmentChart } from '@/hooks/use-analytics';
import type { AnalyticsFilters } from '@/types/analytics';

interface HoursByDepartmentChartProps {
  filters: AnalyticsFilters;
}

export function HoursByDepartmentChart({ filters }: HoursByDepartmentChartProps) {
  const { data, isLoading, isError, error } = useHoursByDepartmentChart(filters);
  const chartData = data ? chartResponseToRechartsData(data) : [];
  const keys = data ? chartDatasetKeys(data) : [];

  return (
    <ChartCard
      title="Hours by Department"
      isLoading={isLoading}
      isError={isError}
      errorMessage={getApiErrorMessage(error, 'Failed to load hours by department.')}
      isEmpty={!isLoading && chartData.length === 0}
    >
      <ResponsiveContainer width="100%" height={280}>
        <BarChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" tick={{ fontSize: 12 }} />
          <YAxis tick={{ fontSize: 12 }} />
          <Tooltip />
          <Legend />
          {keys.map((key, index) => (
            <Bar key={key} dataKey={key} fill={CHART_COLORS[index % CHART_COLORS.length]} />
          ))}
        </BarChart>
      </ResponsiveContainer>
    </ChartCard>
  );
}
