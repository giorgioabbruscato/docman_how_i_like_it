import type { ChartResponseDto } from '@/types/analytics';

export type ChartPoint = Record<string, string | number>;

export function chartResponseToRechartsData(chart: ChartResponseDto): ChartPoint[] {
  if (chart.labels.length === 0) {
    return [];
  }

  return chart.labels.map((label, index) => {
    const point: ChartPoint = { name: label };
    for (const dataset of chart.datasets) {
      point[dataset.label] = dataset.data[index] ?? 0;
    }
    return point;
  });
}

export function chartDatasetKeys(chart: ChartResponseDto): string[] {
  return chart.datasets.map((dataset) => dataset.label);
}

export const CHART_COLORS = ['#2563eb', '#16a34a', '#dc2626', '#9333ea', '#ea580c', '#0891b2'];
