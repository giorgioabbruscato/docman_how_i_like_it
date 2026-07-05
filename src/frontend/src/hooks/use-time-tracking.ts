import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createManualEntry,
  exportTimeEntries,
  getActiveTimer,
  getTimeEntries,
  startTimer,
  stopTimer,
} from '@/api/time-tracking';
import { useTimerStore } from '@/stores/timer-store';
import type {
  CreateManualTimeEntryRequest,
  ExportTimeEntriesQuery,
  StartTimerRequest,
  TimeEntryFilters,
} from '@/types/time-entry';

export function useTimeEntries(filters: TimeEntryFilters) {
  return useQuery({
    queryKey: ['time-entries', filters],
    queryFn: () => getTimeEntries(filters),
  });
}

export function useActiveTimer() {
  const setActiveEntry = useTimerStore((state) => state.setActiveEntry);

  return useQuery({
    queryKey: ['active-timer'],
    queryFn: async () => {
      const entry = await getActiveTimer();
      setActiveEntry(entry);
      return entry;
    },
    refetchInterval: (query) => (query.state.data && !query.state.data.endTime ? 60_000 : false),
  });
}

export function useStartTimer() {
  const queryClient = useQueryClient();
  const setActiveEntry = useTimerStore((state) => state.setActiveEntry);

  return useMutation({
    mutationFn: (input: StartTimerRequest) => startTimer(input),
    onSuccess: (entry) => {
      setActiveEntry(entry);
      void queryClient.invalidateQueries({ queryKey: ['time-entries'] });
      void queryClient.invalidateQueries({ queryKey: ['active-timer'] });
    },
  });
}

export function useStopTimer() {
  const queryClient = useQueryClient();
  const setActiveEntry = useTimerStore((state) => state.setActiveEntry);

  return useMutation({
    mutationFn: () => stopTimer(),
    onSuccess: () => {
      setActiveEntry(null);
      void queryClient.invalidateQueries({ queryKey: ['time-entries'] });
      void queryClient.invalidateQueries({ queryKey: ['active-timer'] });
    },
  });
}

export function useCreateManualEntry() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateManualTimeEntryRequest) => createManualEntry(input),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['time-entries'] });
    },
  });
}

export function useExportTimeEntries() {
  return useMutation({
    mutationFn: (query: ExportTimeEntriesQuery) => exportTimeEntries(query),
  });
}
