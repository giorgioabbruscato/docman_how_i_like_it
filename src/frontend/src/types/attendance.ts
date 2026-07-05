import type { PagedQuery } from '@/types/common';

export interface AttendanceSessionDto {
  id: string;
  employeeId: string;
  checkIn: string;
  checkOut?: string | null;
  latitudeCheckIn?: number | null;
  longitudeCheckIn?: number | null;
  latitudeCheckOut?: number | null;
  longitudeCheckOut?: number | null;
  accuracyCheckIn?: number | null;
  accuracyCheckOut?: number | null;
  device?: string | null;
  browser?: string | null;
  workedMinutes?: number | null;
  status: string;
}

export interface CheckInRequest {
  latitude?: number | null;
  longitude?: number | null;
  accuracy?: number | null;
  timezone?: string | null;
  device?: string | null;
  browser?: string | null;
}

export interface CheckOutRequest {
  latitude?: number | null;
  longitude?: number | null;
  accuracy?: number | null;
  device?: string | null;
  browser?: string | null;
}

export interface CheckOutResponseDto {
  sessionId: string;
  checkIn: string;
  checkOut: string;
  workedMinutes: number;
  status: string;
}

export interface AttendanceDashboardDto {
  todayCheckIn?: string | null;
  todayCheckOut?: string | null;
  todayWorkedMinutes: number;
  currentSession?: AttendanceSessionDto | null;
  weeklyTotalMinutes: number;
  monthlyTotalMinutes: number;
}

export interface AttendanceHistoryQuery extends PagedQuery {
  fromDate?: string;
  toDate?: string;
  employeeId?: string;
}
