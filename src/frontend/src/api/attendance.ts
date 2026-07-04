import { apiClient } from '@/lib/api-client';
import type { AttendanceRecord, AttendanceReport, CheckInOutInput } from '@/types/attendance';

export async function fetchAttendanceRecords(): Promise<AttendanceRecord[]> {
  const { data } = await apiClient.get<AttendanceRecord[]>('/v1/attendance');
  return data;
}

export async function checkIn(input: CheckInOutInput): Promise<AttendanceRecord> {
  const { data } = await apiClient.post<AttendanceRecord>('/v1/attendance/check-in', input);
  return data;
}

export async function checkOut(input: CheckInOutInput): Promise<AttendanceRecord> {
  const { data } = await apiClient.post<AttendanceRecord>('/v1/attendance/check-out', input);
  return data;
}

export async function fetchAttendanceReport(from: string, to: string): Promise<AttendanceReport> {
  const { data } = await apiClient.get<AttendanceReport>('/v1/attendance/reports', {
    params: { from, to },
  });
  return data;
}
