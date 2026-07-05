import { useMutation, useQuery } from '@tanstack/react-query';
import {
  getAttendanceToday,
  getAttendanceTrendChart,
  getBudgetConsumptionChart,
  getBudgetUsage,
  getEmployeesWorking,
  getHoursByDepartmentChart,
  getHoursByEmployeeChart,
  getHoursByMonthChart,
  getHoursByProjectChart,
  getLateArrivals,
  getLeaveTrendChart,
  getOvertime,
  getSupervisorSummary,
  getTopEmployees,
  getTopProjects,
} from '@/api/analytics';
import { downloadReport } from '@/api/reports';
import type { AnalyticsFilters, ReportDownloadParams, ReportType } from '@/types/analytics';

export function useSupervisorSummary(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'supervisor-summary', filters],
    queryFn: () => getSupervisorSummary(filters),
  });
}

export function useEmployeesWorking(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'employees-working', filters],
    queryFn: () => getEmployeesWorking(filters),
  });
}

export function useAttendanceToday(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'attendance-today', filters],
    queryFn: () => getAttendanceToday(filters),
  });
}

export function useTopEmployees(filters: AnalyticsFilters, top = 5) {
  return useQuery({
    queryKey: ['analytics', 'top-employees', filters, top],
    queryFn: () => getTopEmployees(filters, top),
  });
}

export function useTopProjects(filters: AnalyticsFilters, top = 5) {
  return useQuery({
    queryKey: ['analytics', 'top-projects', filters, top],
    queryFn: () => getTopProjects(filters, top),
  });
}

export function useBudgetUsage(filters: AnalyticsFilters, enabled = true) {
  return useQuery({
    queryKey: ['analytics', 'budget-usage', filters],
    queryFn: () => getBudgetUsage(filters),
    enabled,
  });
}

export function useLateArrivals(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'late-arrivals', filters],
    queryFn: () => getLateArrivals(filters),
  });
}

export function useOvertime(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'overtime', filters],
    queryFn: () => getOvertime(filters),
  });
}

export function useHoursByProjectChart(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'hours-by-project', filters],
    queryFn: () => getHoursByProjectChart(filters),
  });
}

export function useHoursByDepartmentChart(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'hours-by-department', filters],
    queryFn: () => getHoursByDepartmentChart(filters),
  });
}

export function useHoursByEmployeeChart(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'hours-by-employee', filters],
    queryFn: () => getHoursByEmployeeChart(filters),
  });
}

export function useHoursByMonthChart(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'hours-by-month', filters],
    queryFn: () => getHoursByMonthChart(filters),
  });
}

export function useAttendanceTrendChart(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'attendance-trend', filters],
    queryFn: () => getAttendanceTrendChart(filters),
  });
}

export function useLeaveTrendChart(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: ['analytics', 'leave-trend', filters],
    queryFn: () => getLeaveTrendChart(filters),
  });
}

export function useBudgetConsumptionChart(filters: AnalyticsFilters, enabled = true) {
  return useQuery({
    queryKey: ['analytics', 'budget-consumption', filters],
    queryFn: () => getBudgetConsumptionChart(filters),
    enabled,
  });
}

export function useDownloadReport() {
  return useMutation({
    mutationFn: ({ type, params }: { type: ReportType; params: ReportDownloadParams }) =>
      downloadReport(type, params),
  });
}
