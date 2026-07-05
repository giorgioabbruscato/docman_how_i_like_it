import { apiClient } from '@/lib/api-client';
import type {
  CreateTimesheetInput,
  GetTimesheetsParams,
  PagedTimesheets,
  RejectTimesheetInput,
  TimesheetSubmission,
} from '@/types/timesheet';

export async function fetchTimesheets(params?: GetTimesheetsParams): Promise<PagedTimesheets> {
  const { data } = await apiClient.get<PagedTimesheets>('/v1/timesheets', { params });
  return data;
}

export async function fetchTimesheet(id: string): Promise<TimesheetSubmission> {
  const { data } = await apiClient.get<TimesheetSubmission>(`/v1/timesheets/${id}`);
  return data;
}

export async function createTimesheet(input: CreateTimesheetInput): Promise<TimesheetSubmission> {
  const { data } = await apiClient.post<TimesheetSubmission>('/v1/timesheets', input);
  return data;
}

export async function submitTimesheet(id: string): Promise<TimesheetSubmission> {
  const { data } = await apiClient.post<TimesheetSubmission>(`/v1/timesheets/${id}/submit`);
  return data;
}

export async function approveTimesheet(id: string): Promise<TimesheetSubmission> {
  const { data } = await apiClient.post<TimesheetSubmission>(`/v1/timesheets/${id}/approve`);
  return data;
}

export async function rejectTimesheet(
  id: string,
  input: RejectTimesheetInput,
): Promise<TimesheetSubmission> {
  const { data } = await apiClient.post<TimesheetSubmission>(`/v1/timesheets/${id}/reject`, input);
  return data;
}
