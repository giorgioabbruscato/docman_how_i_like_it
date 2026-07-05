export type GpsStatus = 'unknown' | 'granted' | 'denied' | 'unavailable';

export interface GeoPosition {
  latitude: number;
  longitude: number;
  accuracy: number;
}

export interface GeoError {
  status: Exclude<GpsStatus, 'unknown' | 'granted'>;
  message: string;
}

const TIMEOUT_MS = 10_000;

export function getCurrentPosition(): Promise<GeoPosition> {
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation) {
      reject({ status: 'unavailable', message: 'Geolocation is not supported by this browser.' } satisfies GeoError);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        resolve({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          accuracy: position.coords.accuracy,
        });
      },
      (error) => {
        if (error.code === error.PERMISSION_DENIED) {
          reject({ status: 'denied', message: 'Location permission denied.' } satisfies GeoError);
        } else {
          reject({ status: 'unavailable', message: error.message || 'Unable to retrieve location.' } satisfies GeoError);
        }
      },
      { enableHighAccuracy: true, timeout: TIMEOUT_MS, maximumAge: 60_000 },
    );
  });
}

export async function probeGeolocationPermission(): Promise<GpsStatus> {
  if (!navigator.geolocation) {
    return 'unavailable';
  }

  if (navigator.permissions?.query) {
    try {
      const result = await navigator.permissions.query({ name: 'geolocation' });
      if (result.state === 'granted') return 'granted';
      if (result.state === 'denied') return 'denied';
    } catch {
      // Permissions API may not support geolocation in all browsers
    }
  }

  return 'unknown';
}
