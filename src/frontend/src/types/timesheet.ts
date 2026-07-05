export type TimesheetStatus = 'Draft' | 'Submitted' | 'Approved' | 'Rejected';

export interface TimesheetApproval {
  id: string;
  decidedBy: string;
  decision: string;
  comment?: string | null;
  decidedAt: string;
}

export interface TimesheetSubmission {
  id: string;
  employeeId: string;
  periodStart: string;
  periodEnd: string;
  totalWorkedMinutes: number;
  status: TimesheetStatus;
  notes?: string | null;
  submittedAt?: string | null;
  timeEntryIds: string[];
  latestApproval?: TimesheetApproval | null;
}

export interface PagedTimesheets {
  items: TimesheetSubmission[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateTimesheetInput {
  periodStart: string;
  periodEnd: string;
  notes?: string | null;
}

export interface RejectTimesheetInput {
  comment?: string | null;
}

export interface GetTimesheetsParams {
  page?: number;
  pageSize?: number;
  employeeId?: string;
  status?: TimesheetStatus;
  fromDate?: string;
  toDate?: string;
}
