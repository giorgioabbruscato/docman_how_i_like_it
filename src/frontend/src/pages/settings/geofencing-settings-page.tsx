import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createGeofenceZone,
  deleteGeofenceZone,
  fetchGeofenceSettings,
  fetchGeofenceZones,
  updateGeofenceSettings,
} from '@/api/geofence';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import { useState } from 'react';

export function GeofencingSettingsPage() {
  const canManage = useHasPermission(Permission.GeofenceManageTenant);
  const queryClient = useQueryClient();
  const { data: zones, isLoading } = useQuery({
    queryKey: ['geofence-zones'],
    queryFn: fetchGeofenceZones,
    enabled: canManage,
  });
  const { data: settings } = useQuery({
    queryKey: ['geofence-settings'],
    queryFn: fetchGeofenceSettings,
    enabled: canManage,
  });

  const [name, setName] = useState('');
  const [latitude, setLatitude] = useState('');
  const [longitude, setLongitude] = useState('');
  const [radius, setRadius] = useState('100');

  const createMutation = useMutation({
    mutationFn: createGeofenceZone,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['geofence-zones'] }),
  });
  const deleteMutation = useMutation({
    mutationFn: deleteGeofenceZone,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['geofence-zones'] }),
  });
  const settingsMutation = useMutation({
    mutationFn: updateGeofenceSettings,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['geofence-settings'] }),
  });

  if (!canManage) {
    return <p className="text-muted-foreground">You do not have permission to manage geofencing.</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Geofencing</h2>
        <p className="text-muted-foreground">Configure check-in zones and GPS policy.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Settings</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={settings?.geofencingEnabled ?? false}
              onChange={(e) =>
                void settingsMutation.mutateAsync({
                  geofencingEnabled: e.target.checked,
                  allowCheckInWithoutGps: settings?.allowCheckInWithoutGps ?? true,
                })
              }
            />
            Enable geofencing
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={settings?.allowCheckInWithoutGps ?? true}
              onChange={(e) =>
                void settingsMutation.mutateAsync({
                  geofencingEnabled: settings?.geofencingEnabled ?? false,
                  allowCheckInWithoutGps: e.target.checked,
                })
              }
            />
            Allow check-in without GPS (audit flag)
          </label>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Add zone</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-3 md:grid-cols-4">
          <div className="space-y-1">
            <label className="text-sm font-medium">Name</label>
            <Input value={name} onChange={(e) => setName(e.target.value)} />
          </div>
          <div className="space-y-1">
            <label className="text-sm font-medium">Latitude</label>
            <Input value={latitude} onChange={(e) => setLatitude(e.target.value)} />
          </div>
          <div className="space-y-1">
            <label className="text-sm font-medium">Longitude</label>
            <Input value={longitude} onChange={(e) => setLongitude(e.target.value)} />
          </div>
          <div className="space-y-1">
            <label className="text-sm font-medium">Radius (m)</label>
            <Input value={radius} onChange={(e) => setRadius(e.target.value)} />
          </div>
          <Button
            className="md:col-span-4"
            onClick={() =>
              void createMutation.mutateAsync({
                name,
                latitude: Number(latitude),
                longitude: Number(longitude),
                radiusMeters: Number(radius),
              })
            }
          >
            Create zone
          </Button>
        </CardContent>
      </Card>

      {isLoading && <LoadingSpinner label="Loading zones" />}
      <div className="grid gap-3">
        {zones?.map((zone) => (
          <Card key={zone.id}>
            <CardContent className="flex items-center justify-between py-4">
              <span>
                {zone.name} — {zone.latitude}, {zone.longitude} ({zone.radiusMeters}m)
              </span>
              <Button
                size="sm"
                variant="outline"
                onClick={() => void deleteMutation.mutateAsync(zone.id)}
              >
                Delete
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
