import { apiClient } from '@/lib/api-client';
import type { PagedResult } from '@/types/common';
import type {
  AttendanceDashboardDto,
  AttendanceHistoryQuery,
  AttendanceSessionDto,
  CheckInRequest,
  CheckOutRequest,
  CheckOutResponseDto,
} from '@/types/attendance';

export async function checkIn(input: CheckInRequest): Promise<AttendanceSessionDto> {
  const { data } = await apiClient.post<AttendanceSessionDto>('/v1/attendance/check-in', input);
  return data;
}

export async function checkOut(input: CheckOutRequest): Promise<CheckOutResponseDto> {
  const { data } = await apiClient.post<CheckOutResponseDto>('/v1/attendance/check-out', input);
  return data;
}

export async function getDashboard(employeeId?: string): Promise<AttendanceDashboardDto> {
  const { data } = await apiClient.get<AttendanceDashboardDto>('/v1/attendance/dashboard', {
    params: employeeId ? { employeeId } : undefined,
  });
  return data;
}

export async function getHistory(
  query: AttendanceHistoryQuery = {},
): Promise<PagedResult<AttendanceSessionDto>> {
  const { data } = await apiClient.get<PagedResult<AttendanceSessionDto>>('/v1/attendance/history', {
    params: query,
  });
  return data;
}
