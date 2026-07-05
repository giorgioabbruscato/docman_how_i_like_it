import { useEffect, useState } from 'react';
import { formatDateTime, formatElapsed } from '@/lib/utils';
import type { AttendanceSessionDto } from '@/types/attendance';

interface LiveTimerProps {
  session: AttendanceSessionDto;
}

export function LiveTimer({ session }: LiveTimerProps) {
  const [elapsedMinutes, setElapsedMinutes] = useState(0);

  useEffect(() => {
    const tick = () => {
      const start = new Date(session.checkIn).getTime();
      const elapsed = Math.max(0, Math.floor((Date.now() - start) / 60_000));
      setElapsedMinutes(elapsed);
    };
    tick();
    const interval = window.setInterval(tick, 1000);
    return () => window.clearInterval(interval);
  }, [session.checkIn]);

  return (
    <div className="rounded-md border border-primary/20 bg-primary/5 p-6 text-center">
      <p className="text-sm text-muted-foreground">Session in progress since {formatDateTime(session.checkIn)}</p>
      <p className="mt-2 font-mono text-4xl font-bold">{formatElapsed(elapsedMinutes * 60)}</p>
      <p className="mt-1 text-xs text-muted-foreground">Live session timer</p>
    </div>
  );
}
