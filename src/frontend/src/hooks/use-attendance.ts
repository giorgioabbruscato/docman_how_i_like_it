import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { checkIn, checkOut, getDashboard, getHistory } from '@/api/attendance';
import type { AttendanceHistoryQuery, CheckInRequest, CheckOutRequest } from '@/types/attendance';

export function useAttendanceDashboard(employeeId?: string) {
  return useQuery({
    queryKey: ['attendance-dashboard', employeeId],
    queryFn: () => getDashboard(employeeId),
  });
}

export function useAttendanceHistory(query: AttendanceHistoryQuery) {
  return useQuery({
    queryKey: ['attendance-history', query],
    queryFn: () => getHistory(query),
  });
}

export function useCheckIn() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CheckInRequest) => checkIn(input),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['attendance-dashboard'] });
      void queryClient.invalidateQueries({ queryKey: ['attendance-history'] });
    },
  });
}

export function useCheckOut() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CheckOutRequest) => checkOut(input),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['attendance-dashboard'] });
      void queryClient.invalidateQueries({ queryKey: ['attendance-history'] });
    },
  });
}
