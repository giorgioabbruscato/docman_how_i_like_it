import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  approveTimesheet,
  createTimesheet,
  fetchTimesheet,
  fetchTimesheets,
  rejectTimesheet,
  submitTimesheet,
} from '@/api/timesheets';
import type {
  CreateTimesheetInput,
  GetTimesheetsParams,
  RejectTimesheetInput,
} from '@/types/timesheet';

export function useTimesheets(params?: GetTimesheetsParams) {
  return useQuery({
    queryKey: ['timesheets', params],
    queryFn: () => fetchTimesheets(params),
  });
}

export function useTimesheet(id: string) {
  return useQuery({
    queryKey: ['timesheets', id],
    queryFn: () => fetchTimesheet(id),
    enabled: Boolean(id),
  });
}

export function useCreateTimesheet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateTimesheetInput) => createTimesheet(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['timesheets'] }),
  });
}

export function useSubmitTimesheet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => submitTimesheet(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['timesheets'] }),
  });
}

export function useApproveTimesheet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => approveTimesheet(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['timesheets'] }),
  });
}

export function useRejectTimesheet() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: RejectTimesheetInput }) =>
      rejectTimesheet(id, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['timesheets'] }),
  });
}
