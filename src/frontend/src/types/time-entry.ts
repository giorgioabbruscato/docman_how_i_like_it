import type { PagedQuery } from '@/types/common';

export interface TimeEntryDto {
  id: string;
  employeeId: string;
  projectId: string;
  taskId?: string | null;
  startTime: string;
  endTime?: string | null;
  workedMinutes: number;
  description?: string | null;
  billable: boolean;
}

export interface CreateTimeEntryRequest {
  projectId: string;
  startTime: string;
  endTime: string;
  taskId?: string | null;
  description?: string | null;
  billable?: boolean;
}

export interface StartTimerRequest {
  projectId: string;
  taskId?: string | null;
  description?: string | null;
  billable?: boolean;
}

export interface CreateManualTimeEntryRequest {
  date: string;
  projectId: string;
  hours: number;
  taskId?: string | null;
  description?: string | null;
  billable?: boolean;
}

export type ExportFormat = 'csv' | 'xlsx' | 'pdf';

export interface TimeEntryFilters extends PagedQuery {
  employeeId?: string;
  projectId?: string;
  taskId?: string;
  fromDate?: string;
  toDate?: string;
  billable?: boolean;
}

export interface ExportTimeEntriesQuery {
  format: ExportFormat;
  employeeId?: string;
  projectId?: string;
  fromDate?: string;
  toDate?: string;
  month?: number;
  year?: number;
}
