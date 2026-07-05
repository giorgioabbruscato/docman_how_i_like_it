import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

export function CalendarCallbackPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const success = searchParams.get('success') === 'true';
  const error = searchParams.get('error');

  useEffect(() => {
    const timer = setTimeout(() => navigate('/settings', { replace: true }), 2500);
    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="flex min-h-[40vh] items-center justify-center">
      <Card className="max-w-md">
        <CardHeader>
          <CardTitle>Calendar connection</CardTitle>
        </CardHeader>
        <CardContent>
          {success ? (
            <p className="text-sm text-muted-foreground">
              Your calendar was connected successfully. Redirecting to settings…
            </p>
          ) : (
            <p className="text-sm text-destructive">
              {error ?? 'Calendar connection failed.'} Redirecting to settings…
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
