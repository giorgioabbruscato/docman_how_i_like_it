export interface AnalyticsFilters {
  departmentId?: string;
  projectId?: string;
  employeeId?: string;
  fromDate?: string;
  toDate?: string;
}

export interface EmployeeWorkingDto {
  employeeId: string;
  employeeName: string;
  projectId: string | null;
  projectName: string | null;
  checkInTime: string | null;
}

export interface AttendanceTodayDto {
  employeeId: string;
  employeeName: string;
  checkInTime: string;
}

export interface TopEmployeeDto {
  employeeId: string;
  employeeName: string;
  hours: number;
}

export interface TopProjectDto {
  projectId: string;
  projectName: string;
  hours: number;
}

export interface BudgetUsageDto {
  projectId: string;
  projectName: string;
  budgetHours: number | null;
  spentHours: number;
  budgetCost: number | null;
  actualCost: number | null;
}

export interface LateArrivalDto {
  employeeId: string;
  employeeName: string;
  checkInTime: string;
}

export interface OvertimeEmployeeDto {
  employeeId: string;
  employeeName: string;
  overtimeHours: number;
}

export interface SupervisorSummaryDto {
  employeesWorking: EmployeeWorkingDto[];
  attendanceToday: AttendanceTodayDto[];
  topEmployees: TopEmployeeDto[];
  topProjects: TopProjectDto[];
  budgetUsage: BudgetUsageDto[];
  lateArrivals: LateArrivalDto[];
  overtime: OvertimeEmployeeDto[];
  totalWorkedHours: number;
  attendanceRate: number;
  leaveRate: number;
}

export interface ChartDatasetDto {
  label: string;
  data: number[];
}

export interface ChartResponseDto {
  labels: string[];
  datasets: ChartDatasetDto[];
}

export type ChartType =
  | 'hours-by-project'
  | 'hours-by-department'
  | 'hours-by-employee'
  | 'hours-by-month'
  | 'attendance-trend'
  | 'leave-trend'
  | 'budget-consumption';

export type ReportType = 'attendance' | 'worked-hours' | 'employees' | 'projects' | 'departments';

export type ReportFormat = 'csv' | 'xlsx' | 'pdf';

export interface ReportDownloadParams extends AnalyticsFilters {
  format: ReportFormat;
}
