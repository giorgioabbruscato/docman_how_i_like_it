import { apiClient } from '@/lib/api-client';

export interface GeofenceZone {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  radiusMeters: number;
  isActive: boolean;
  description?: string | null;
}

export interface GeofenceSettings {
  geofencingEnabled: boolean;
  allowCheckInWithoutGps: boolean;
}

export async function fetchGeofenceZones(): Promise<GeofenceZone[]> {
  const { data } = await apiClient.get<GeofenceZone[]>('/v1/geofence-zones');
  return data;
}

export async function createGeofenceZone(input: {
  name: string;
  latitude: number;
  longitude: number;
  radiusMeters: number;
  description?: string;
}): Promise<GeofenceZone> {
  const { data } = await apiClient.post<GeofenceZone>('/v1/geofence-zones', input);
  return data;
}

export async function deleteGeofenceZone(id: string): Promise<void> {
  await apiClient.delete(`/v1/geofence-zones/${id}`);
}

export async function fetchGeofenceSettings(): Promise<GeofenceSettings> {
  const { data } = await apiClient.get<GeofenceSettings>('/v1/geofence-zones/settings');
  return data;
}

export async function updateGeofenceSettings(input: GeofenceSettings): Promise<GeofenceSettings> {
  const { data } = await apiClient.put<GeofenceSettings>('/v1/geofence-zones/settings', input);
  return data;
}
