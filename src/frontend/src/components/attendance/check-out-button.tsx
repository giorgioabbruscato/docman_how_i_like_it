import { LogOut } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { getDeviceInfo } from '@/lib/device-info';
import { getCurrentPosition, type GeoError } from '@/lib/geolocation';
import { useCheckOut } from '@/hooks/use-attendance';
import { getApiErrorMessage } from '@/lib/utils';
import { useAttendanceStore } from '@/stores/attendance-store';

interface CheckOutButtonProps {
  onSuccess: () => void;
  onError: (message: string) => void;
}

export function CheckOutButton({ onSuccess, onError }: CheckOutButtonProps) {
  const checkOut = useCheckOut();
  const { setGpsStatus, setSuccessMessage } = useAttendanceStore();

  const handleCheckOut = async () => {
    const { device, browser } = getDeviceInfo();

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
      await checkOut.mutateAsync({
        latitude,
        longitude,
        accuracy,
        device,
        browser,
      });
      setSuccessMessage('Checked out successfully.');
      onSuccess();
    } catch (err) {
      onError(getApiErrorMessage(err, 'Failed to check out.'));
    }
  };

  return (
    <Button
      variant="destructive"
      className="min-h-16 w-full text-lg"
      onClick={() => void handleCheckOut()}
      disabled={checkOut.isPending}
    >
      <LogOut className="mr-2 h-5 w-5" />
      {checkOut.isPending ? 'Checking out...' : 'Check Out'}
    </Button>
  );
}
