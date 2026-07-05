import { apiClient } from '@/lib/api-client';

export interface CalendarEvent {
  id: string;
  title: string;
  startDate: string;
  endDate: string;
  type: string;
  employeeId?: string | null;
  employeeName?: string | null;
  color: string;
}

export interface PublicHoliday {
  id: string;
  name: string;
  date: string;
  isRecurring: boolean;
  countryCode?: string | null;
}

export async function fetchCalendarEvents(params: {
  fromDate: string;
  toDate: string;
  departmentId?: string;
  employeeId?: string;
}): Promise<CalendarEvent[]> {
  const { data } = await apiClient.get<CalendarEvent[]>('/v1/calendar/events', { params });
  return data;
}

export async function fetchHolidays(): Promise<PublicHoliday[]> {
  const { data } = await apiClient.get<PublicHoliday[]>('/v1/calendar/holidays');
  return data;
}

export async function createHoliday(input: {
  name: string;
  date: string;
  isRecurring?: boolean;
  countryCode?: string;
}): Promise<PublicHoliday> {
  const { data } = await apiClient.post<PublicHoliday>('/v1/calendar/holidays', input);
  return data;
}

export async function deleteHoliday(id: string): Promise<void> {
  await apiClient.delete(`/v1/calendar/holidays/${id}`);
}
