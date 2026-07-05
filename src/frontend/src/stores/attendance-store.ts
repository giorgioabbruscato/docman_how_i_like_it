import { create } from 'zustand';
import { probeGeolocationPermission, type GpsStatus } from '@/lib/geolocation';

interface AttendanceState {
  gpsStatus: GpsStatus;
  successMessage: string | null;
  setGpsStatus: (status: GpsStatus) => void;
  setSuccessMessage: (message: string | null) => void;
  probeGps: () => Promise<void>;
}

export const useAttendanceStore = create<AttendanceState>()((set) => ({
  gpsStatus: 'unknown',
  successMessage: null,
  setGpsStatus: (gpsStatus) => set({ gpsStatus }),
  setSuccessMessage: (successMessage) => set({ successMessage }),
  probeGps: async () => {
    const status = await probeGeolocationPermission();
    set({ gpsStatus: status });
  },
}));
