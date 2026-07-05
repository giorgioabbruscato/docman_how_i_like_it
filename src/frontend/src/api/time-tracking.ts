import { apiClient } from '@/lib/api-client';
import type { PagedResult } from '@/types/common';
import type {
  CreateManualTimeEntryRequest,
  ExportTimeEntriesQuery,
  StartTimerRequest,
  TimeEntryDto,
  TimeEntryFilters,
} from '@/types/time-entry';

export async function getTimeEntries(
  filters: TimeEntryFilters = {},
): Promise<PagedResult<TimeEntryDto>> {
  const { data } = await apiClient.get<PagedResult<TimeEntryDto>>('/v1/time-entries', {
    params: filters,
  });
  return data;
}

export async function getActiveTimer(): Promise<TimeEntryDto | null> {
  try {
    const { data } = await apiClient.get<TimeEntryDto>('/v1/timer/active');
    return data;
  } catch (error: unknown) {
    const status = (error as { response?: { status?: number } }).response?.status;
    if (status === 404) return null;
    throw error;
  }
}

export async function startTimer(input: StartTimerRequest): Promise<TimeEntryDto> {
  const { data } = await apiClient.post<TimeEntryDto>('/v1/timer/start', input);
  return data;
}

export async function stopTimer(): Promise<TimeEntryDto> {
  const { data } = await apiClient.post<TimeEntryDto>('/v1/timer/stop');
  return data;
}

export async function createManualEntry(
  input: CreateManualTimeEntryRequest,
): Promise<TimeEntryDto> {
  const { data } = await apiClient.post<TimeEntryDto>('/v1/time-entries/manual', input);
  return data;
}

export async function exportTimeEntries(query: ExportTimeEntriesQuery): Promise<Blob> {
  const { data } = await apiClient.get<Blob>('/v1/time-entries/export', {
    params: query,
    responseType: 'blob',
  });
  return data;
}
