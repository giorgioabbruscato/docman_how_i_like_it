import { AlertTriangle, CheckCircle2, HelpCircle, MapPinOff } from 'lucide-react';
import type { GpsStatus } from '@/lib/geolocation';
import { cn } from '@/lib/utils';

const statusConfig: Record<
  GpsStatus,
  { label: string; icon: typeof CheckCircle2; className: string }
> = {
  granted: {
    label: 'GPS available',
    icon: CheckCircle2,
    className: 'border-green-200 bg-green-50 text-green-800',
  },
  denied: {
    label: 'GPS denied',
    icon: MapPinOff,
    className: 'border-yellow-200 bg-yellow-50 text-yellow-800',
  },
  unavailable: {
    label: 'GPS unavailable',
    icon: AlertTriangle,
    className: 'border-orange-200 bg-orange-50 text-orange-800',
  },
  unknown: {
    label: 'GPS status unknown',
    icon: HelpCircle,
    className: 'border-border bg-muted/50 text-muted-foreground',
  },
};

export function GpsStatus({ status }: { status: GpsStatus }) {
  const config = statusConfig[status];
  const Icon = config.icon;

  return (
    <div className={cn('flex items-center gap-2 rounded-md border px-3 py-2 text-sm', config.className)}>
      <Icon className="h-4 w-4 shrink-0" />
      <span>{config.label}</span>
    </div>
  );
}
