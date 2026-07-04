export type LeaveType =
  | 'Annual'
  | 'Sick'
  | 'Personal'
  | 'Maternity'
  | 'Paternity'
  | 'Unpaid';

export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';

export interface LeaveRequest {
  id: string;
  employeeId: string;
  startDate: string;
  endDate: string;
  type: LeaveType | string;
  status: LeaveStatus | string;
  reason?: string;
  approvedBy?: string;
  approvedAt?: string;
}

export interface CreateLeaveRequestInput {
  employeeId: string;
  startDate: string;
  endDate: string;
  type: LeaveType | string;
  reason?: string;
}

export interface RejectLeaveRequestInput {
  reason?: string;
}

export const LEAVE_TYPES: LeaveType[] = [
  'Annual',
  'Sick',
  'Personal',
  'Maternity',
  'Paternity',
  'Unpaid',
];
