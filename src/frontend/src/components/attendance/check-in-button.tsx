import { LogIn } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { getDeviceInfo } from '@/lib/device-info';
import { getCurrentPosition, type GeoError } from '@/lib/geolocation';
import { useCheckIn } from '@/hooks/use-attendance';
import { getApiErrorMessage } from '@/lib/utils';
import { useAttendanceStore } from '@/stores/attendance-store';

interface CheckInButtonProps {
  onSuccess: () => void;
  onError: (message: string) => void;
}

export function CheckInButton({ onSuccess, onError }: CheckInButtonProps) {
  const checkIn = useCheckIn();
  const { setGpsStatus, setSuccessMessage } = useAttendanceStore();

  const handleCheckIn = async () => {
    const { device, browser } = getDeviceInfo();
    const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    let latitude: number | undefined;
    let longitude: number | undefined;
    let accuracy: number | undefined;

    try {
      const position = await getCurrentPosition();
      latitude = position.latitude;
      longitude = position.longitude;
      accuracy = position.accuracy;
      setGpsStatus('granted');
    } catch (err) {
      const geoError = err as GeoError;
      setGpsStatus(geoError.status);
    }

    try {
      await checkIn.mutateAsync({
        latitude,
        longitude,
        accuracy,
        timezone,
        device,
        browser,
      });
      setSuccessMessage('Checked in successfully.');
      onSuccess();
    } catch (err) {
      onError(getApiErrorMessage(err, 'Failed to check in.'));
    }
  };

  return (
    <Button
      className="min-h-16 w-full bg-green-600 text-lg hover:bg-green-700"
      onClick={() => void handleCheckIn()}
      disabled={checkIn.isPending}
    >
      <LogIn className="mr-2 h-5 w-5" />
      {checkIn.isPending ? 'Checking in...' : 'Check In'}
    </Button>
  );
}
