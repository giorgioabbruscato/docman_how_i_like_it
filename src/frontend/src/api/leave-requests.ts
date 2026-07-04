import { apiClient } from '@/lib/api-client';
import type {
  CreateLeaveRequestInput,
  LeaveRequest,
  RejectLeaveRequestInput,
} from '@/types/leave-request';

export async function fetchLeaveRequests(): Promise<LeaveRequest[]> {
  const { data } = await apiClient.get<LeaveRequest[]>('/v1/leave-requests');
  return data;
}

export async function fetchLeaveRequest(id: string): Promise<LeaveRequest> {
  const { data } = await apiClient.get<LeaveRequest>(`/v1/leave-requests/${id}`);
  return data;
}

export async function createLeaveRequest(input: CreateLeaveRequestInput): Promise<LeaveRequest> {
  const { data } = await apiClient.post<LeaveRequest>('/v1/leave-requests', input);
  return data;
}

export async function approveLeaveRequest(id: string): Promise<LeaveRequest> {
  const { data } = await apiClient.put<LeaveRequest>(`/v1/leave-requests/${id}/approve`);
  return data;
}

export async function rejectLeaveRequest(
  id: string,
  input: RejectLeaveRequestInput,
): Promise<LeaveRequest> {
  const { data } = await apiClient.put<LeaveRequest>(`/v1/leave-requests/${id}/reject`, input);
  return data;
}

export async function cancelLeaveRequest(id: string): Promise<void> {
  await apiClient.delete(`/v1/leave-requests/${id}`);
}
