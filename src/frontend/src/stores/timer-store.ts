import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { getActiveTimer } from '@/api/time-tracking';
import type { TimeEntryDto } from '@/types/time-entry';

interface TimerState {
  activeEntry: TimeEntryDto | null;
  setActiveEntry: (entry: TimeEntryDto | null) => void;
  syncActiveTimer: () => Promise<void>;
}

export const useTimerStore = create<TimerState>()(
  persist(
    (set) => ({
      activeEntry: null,
      setActiveEntry: (activeEntry) => set({ activeEntry }),
      syncActiveTimer: async () => {
        const entry = await getActiveTimer();
        set({ activeEntry: entry });
      },
    }),
    {
      name: 'hrportal-timer',
      partialize: (state) => ({ activeEntry: state.activeEntry }),
    },
  ),
);

export function getElapsedSeconds(entry: TimeEntryDto | null): number {
  if (!entry) return 0;
  const start = new Date(entry.startTime).getTime();
  const end = entry.endTime ? new Date(entry.endTime).getTime() : Date.now();
  return Math.max(0, Math.floor((end - start) / 1000));
}
