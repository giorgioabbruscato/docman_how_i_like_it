import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { ChartCard } from '@/components/analytics/chart-card';
import { CHART_COLORS, chartDatasetKeys, chartResponseToRechartsData } from '@/lib/chart-utils';
import { getApiErrorMessage } from '@/lib/utils';
import { useHoursByMonthChart } from '@/hooks/use-analytics';
import type { AnalyticsFilters } from '@/types/analytics';

interface MonthlyTrendChartProps {
  filters: AnalyticsFilters;
}

export function MonthlyTrendChart({ filters }: MonthlyTrendChartProps) {
  const { data, isLoading, isError, error } = useHoursByMonthChart(filters);
  const chartData = data ? chartResponseToRechartsData(data) : [];
  const keys = data ? chartDatasetKeys(data) : [];

  return (
    <ChartCard
      title="Monthly Hours Trend"
      isLoading={isLoading}
      isError={isError}
      errorMessage={getApiErrorMessage(error, 'Failed to load monthly trend.')}
      isEmpty={!isLoading && chartData.length === 0}
    >
      <ResponsiveContainer width="100%" height={280}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" tick={{ fontSize: 12 }} />
          <YAxis tick={{ fontSize: 12 }} />
          <Tooltip />
          <Legend />
          {keys.map((key, index) => (
            <Line
              key={key}
              type="monotone"
              dataKey={key}
              stroke={CHART_COLORS[index % CHART_COLORS.length]}
              strokeWidth={2}
              dot={{ r: 3 }}
            />
          ))}
        </LineChart>
      </ResponsiveContainer>
    </ChartCard>
  );
}
