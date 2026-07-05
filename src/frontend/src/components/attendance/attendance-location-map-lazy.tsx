import { lazy, Suspense } from 'react';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import type { AttendanceLocationMapProps } from './attendance-location-map';

export const LazyAttendanceLocationMap = lazy(() =>
  import('./attendance-location-map').then((module) => ({ default: module.AttendanceLocationMap })),
);

export function AttendanceLocationMapLazy(props: AttendanceLocationMapProps) {
  return (
    <Suspense
      fallback={
        <div className="flex h-[240px] w-full items-center justify-center rounded-md border border-border md:h-[320px]">
          <LoadingSpinner label="Loading map" />
        </div>
      }
    >
      <LazyAttendanceLocationMap {...props} />
    </Suspense>
  );
}
