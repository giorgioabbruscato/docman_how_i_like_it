import { apiClient } from '@/lib/api-client';
import type {
  AnalyticsFilters,
  AttendanceTodayDto,
  BudgetUsageDto,
  ChartResponseDto,
  EmployeeWorkingDto,
  LateArrivalDto,
  OvertimeEmployeeDto,
  SupervisorSummaryDto,
  TopEmployeeDto,
  TopProjectDto,
} from '@/types/analytics';

function buildParams(filters: AnalyticsFilters, extra?: Record<string, unknown>) {
  return {
    departmentId: filters.departmentId,
    projectId: filters.projectId,
    employeeId: filters.employeeId,
    fromDate: filters.fromDate,
    toDate: filters.toDate,
    ...extra,
  };
}

export async function getSupervisorSummary(
  filters: AnalyticsFilters = {},
): Promise<SupervisorSummaryDto> {
  const { data } = await apiClient.get<SupervisorSummaryDto>('/v1/analytics/supervisor/summary', {
    params: buildParams(filters),
  });
  return data;
}

export async function getEmployeesWorking(
  filters: AnalyticsFilters = {},
): Promise<EmployeeWorkingDto[]> {
  const { data } = await apiClient.get<EmployeeWorkingDto[]>(
    '/v1/analytics/supervisor/employees-working',
    { params: buildParams(filters) },
  );
  return data;
}

export async function getAttendanceToday(
  filters: AnalyticsFilters = {},
): Promise<AttendanceTodayDto[]> {
  const { data } = await apiClient.get<AttendanceTodayDto[]>(
    '/v1/analytics/supervisor/attendance-today',
    { params: buildParams(filters) },
  );
  return data;
}

export async function getTopEmployees(
  filters: AnalyticsFilters = {},
  top = 5,
): Promise<TopEmployeeDto[]> {
  const { data } = await apiClient.get<TopEmployeeDto[]>('/v1/analytics/supervisor/top-employees', {
    params: buildParams(filters, { top }),
  });
  return data;
}

export async function getTopProjects(
  filters: AnalyticsFilters = {},
  top = 5,
): Promise<TopProjectDto[]> {
  const { data } = await apiClient.get<TopProjectDto[]>('/v1/analytics/supervisor/top-projects', {
    params: buildParams(filters, { top }),
  });
  return data;
}

export async function getBudgetUsage(filters: AnalyticsFilters = {}): Promise<BudgetUsageDto[]> {
  const { data } = await apiClient.get<BudgetUsageDto[]>('/v1/analytics/supervisor/budget-usage', {
    params: buildParams(filters),
  });
  return data;
}

export async function getLateArrivals(filters: AnalyticsFilters = {}): Promise<LateArrivalDto[]> {
  const { data } = await apiClient.get<LateArrivalDto[]>('/v1/analytics/supervisor/late-arrivals', {
    params: buildParams(filters),
  });
  return data;
}

export async function getOvertime(filters: AnalyticsFilters = {}): Promise<OvertimeEmployeeDto[]> {
  const { data } = await apiClient.get<OvertimeEmployeeDto[]>('/v1/analytics/supervisor/overtime', {
    params: buildParams(filters),
  });
  return data;
}

export async function getHoursByProjectChart(
  filters: AnalyticsFilters = {},
): Promise<ChartResponseDto> {
  const { data } = await apiClient.get<ChartResponseDto>('/v1/analytics/charts/hours-by-project', {
    params: buildParams(filters),
  });
  return data;
}

export async function getHoursByDepartmentChart(
  filters: AnalyticsFilters = {},
): Promise<ChartResponseDto> {
  const { data } = await apiClient.get<ChartResponseDto>(
    '/v1/analytics/charts/hours-by-department',
    { params: buildParams(filters) },
  );
  return data;
}

export async function getHoursByEmployeeChart(
  filters: AnalyticsFilters = {},
): Promise<ChartResponseDto> {
  const { data } = await apiClient.get<ChartResponseDto>('/v1/analytics/charts/hours-by-employee', {
    params: buildParams(filters),
  });
  return data;
}

export async function getHoursByMonthChart(
  filters: AnalyticsFilters = {},
): Promise<ChartResponseDto> {
  const { data } = await apiClient.get<ChartResponseDto>('/v1/analytics/charts/hours-by-month', {
    params: buildParams(filters),
  });
  return data;
}

export async function getAttendanceTrendChart(
  filters: AnalyticsFilters = {},
): Promise<ChartResponseDto> {
  const { data } = await apiClient.get<ChartResponseDto>('/v1/analytics/charts/attendance-trend', {
    params: buildParams(filters),
  });
  return data;
}

export async function getLeaveTrendChart(
  filters: AnalyticsFilters = {},
): Promise<ChartResponseDto> {
  const { data } = await apiClient.get<ChartResponseDto>('/v1/analytics/charts/leave-trend', {
    params: buildParams(filters),
  });
  return data;
}

export async function getBudgetConsumptionChart(
  filters: AnalyticsFilters = {},
): Promise<ChartResponseDto> {
  const { data } = await apiClient.get<ChartResponseDto>(
    '/v1/analytics/charts/budget-consumption',
    { params: buildParams(filters) },
  );
  return data;
}
