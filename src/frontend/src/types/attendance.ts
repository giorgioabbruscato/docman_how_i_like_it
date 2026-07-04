export type AttendanceStatus = 'Present' | 'Absent' | 'Late' | 'HalfDay' | 'Remote';

export interface AttendanceRecord {
  id: string;
  employeeId: string;
  date: string;
  checkIn?: string;
  checkOut?: string;
  status: AttendanceStatus | string;
  notes?: string;
}

export interface CheckInOutInput {
  employeeId: string;
  date?: string;
  time?: string;
}

export interface AttendanceReport {
  from: string;
  to: string;
  totalRecords: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  halfDayCount: number;
  remoteCount: number;
}
