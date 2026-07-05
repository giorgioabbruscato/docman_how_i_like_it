import { apiClient } from '@/lib/api-client';

export interface CalendarProvider {
  id: string;
  name: string;
}

export interface CalendarConnection {
  id: string;
  provider: string;
  connectedAt: string;
  isActive: boolean;
}

export interface CalendarSyncLogEntry {
  id: string;
  leaveRequestId: string;
  employeeId?: string;
  provider?: string;
  status: string;
  message?: string;
  retryCount: number;
  nextRetryAt?: string;
  createdAt: string;
}

export async function fetchCalendarProviders(): Promise<CalendarProvider[]> {
  const { data } = await apiClient.get<CalendarProvider[]>('/v1/integrations/calendar/providers');
  return data;
}

export async function getCalendarConnectUrl(
  provider: string,
  redirectUri: string,
): Promise<{ authorizationUrl: string }> {
  const { data } = await apiClient.get<{ authorizationUrl: string }>(
    `/v1/integrations/calendar/connect/${provider}`,
    { params: { redirectUri } },
  );
  return data;
}

export async function fetchCalendarConnections(): Promise<CalendarConnection[]> {
  const { data } = await apiClient.get<CalendarConnection[]>('/v1/integrations/calendar/connections');
  return data;
}

export async function disconnectCalendar(connectionId: string): Promise<void> {
  await apiClient.delete(`/v1/integrations/calendar/connections/${connectionId}`);
}

export async function fetchCalendarSyncLog(limit = 50): Promise<CalendarSyncLogEntry[]> {
  const { data } = await apiClient.get<CalendarSyncLogEntry[]>('/v1/integrations/calendar/sync-log', {
    params: { limit },
  });
  return data;
}
