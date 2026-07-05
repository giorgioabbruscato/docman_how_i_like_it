import { useEffect, useRef } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import type { GeofenceZone } from '@/api/geofence';
import { cn, formatDateTime } from '@/lib/utils';

export interface AttendanceLocationMapProps {
  checkInLat?: number | null;
  checkInLng?: number | null;
  checkOutLat?: number | null;
  checkOutLng?: number | null;
  checkInAt?: string | null;
  checkOutAt?: string | null;
  geofenceZones?: GeofenceZone[];
  className?: string;
}

function hasCoords(lat?: number | null, lng?: number | null): boolean {
  return lat != null && lng != null && !Number.isNaN(lat) && !Number.isNaN(lng);
}

function formatCoords(lat: number, lng: number): string {
  return `${lat.toFixed(5)}, ${lng.toFixed(5)}`;
}

function buildPopupHtml(label: string, at: string | null | undefined, lat: number, lng: number): string {
  const lines = [`<strong>${label}</strong>`];
  if (at) {
    lines.push(formatDateTime(at));
  }
  lines.push(formatCoords(lat, lng));
  return lines.join('<br/>');
}

export function AttendanceLocationMap({
  checkInLat,
  checkInLng,
  checkOutLat,
  checkOutLng,
  checkInAt,
  checkOutAt,
  geofenceZones,
  className,
}: AttendanceLocationMapProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<L.Map | null>(null);

  const hasCheckIn = hasCoords(checkInLat, checkInLng);
  const hasCheckOut = hasCoords(checkOutLat, checkOutLng);
  const hasAnyLocation = hasCheckIn || hasCheckOut;

  useEffect(() => {
    if (!containerRef.current || !hasAnyLocation) {
      return;
    }

    const map = L.map(containerRef.current, { scrollWheelZoom: false });
    mapRef.current = map;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution:
        '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19,
    }).addTo(map);

    geofenceZones
      ?.filter((zone) => zone.isActive)
      .forEach((zone) => {
        L.circle([zone.latitude, zone.longitude], {
          radius: zone.radiusMeters,
          color: '#3b82f6',
          fillColor: '#3b82f6',
          fillOpacity: 0.1,
          weight: 2,
        })
          .addTo(map)
          .bindPopup(zone.name);
      });

    const bounds = L.latLngBounds([]);

    if (hasCheckIn) {
      const latlng: L.LatLngExpression = [checkInLat!, checkInLng!];
      L.circleMarker(latlng, {
        radius: 8,
        color: '#15803d',
        fillColor: '#22c55e',
        fillOpacity: 1,
        weight: 2,
      })
        .addTo(map)
        .bindPopup(buildPopupHtml('Check-in', checkInAt, checkInLat!, checkInLng!));
      bounds.extend(latlng);
    }

    if (hasCheckOut) {
      const latlng: L.LatLngExpression = [checkOutLat!, checkOutLng!];
      L.circleMarker(latlng, {
        radius: 8,
        color: '#b91c1c',
        fillColor: '#ef4444',
        fillOpacity: 1,
        weight: 2,
      })
        .addTo(map)
        .bindPopup(buildPopupHtml('Check-out', checkOutAt, checkOutLat!, checkOutLng!));
      bounds.extend(latlng);
    }

    if (hasCheckIn && hasCheckOut) {
      map.fitBounds(bounds, { padding: [24, 24] });
    } else if (hasCheckIn) {
      map.setView([checkInLat!, checkInLng!], 15);
    } else if (hasCheckOut) {
      map.setView([checkOutLat!, checkOutLng!], 15);
    }

    return () => {
      map.remove();
      mapRef.current = null;
    };
  }, [
    checkInAt,
    checkInLat,
    checkInLng,
    checkOutAt,
    checkOutLat,
    checkOutLng,
    geofenceZones,
    hasAnyLocation,
    hasCheckIn,
    hasCheckOut,
  ]);

  if (!hasAnyLocation) {
    return (
      <div
        className={cn(
          'flex h-[240px] w-full items-center justify-center rounded-md border border-dashed border-border bg-muted/30 md:h-[320px]',
          className,
        )}
        role="status"
      >
        <p className="text-sm text-muted-foreground">Location not recorded</p>
        <span className="sr-only">
          No GPS coordinates were captured for this attendance session.
        </span>
      </div>
    );
  }

  const ariaLabelParts: string[] = [];
  if (hasCheckIn) {
    ariaLabelParts.push(
      `Check-in at ${formatCoords(checkInLat!, checkInLng!)}${checkInAt ? ` on ${formatDateTime(checkInAt)}` : ''}`,
    );
  }
  if (hasCheckOut) {
    ariaLabelParts.push(
      `Check-out at ${formatCoords(checkOutLat!, checkOutLng!)}${checkOutAt ? ` on ${formatDateTime(checkOutAt)}` : ''}`,
    );
  }
  const ariaLabel = `Attendance location map. ${ariaLabelParts.join('. ')}.`;

  return (
    <div className={cn('relative w-full', className)}>
      <span className="sr-only">{ariaLabel}</span>
      <div
        ref={containerRef}
        className="h-[240px] w-full overflow-hidden rounded-md md:h-[320px]"
        role="img"
        aria-label={ariaLabel}
      />
    </div>
  );
}
