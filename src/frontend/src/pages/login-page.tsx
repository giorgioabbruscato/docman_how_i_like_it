import { useEffect } from 'react';
import { keycloak } from '@/lib/keycloak';
import { useAuth } from '@/providers/auth-provider';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';

export function LoginPage() {
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      window.location.replace('/');
    }
  }, [isAuthenticated, isLoading]);

  const handleLogin = () => {
    void keycloak.login({ redirectUri: `${window.location.origin}/` });
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-muted/30 p-4">
      <Card className="w-full max-w-md p-8 space-y-6">
        <div className="space-y-2 text-center">
          <h1 className="text-2xl font-bold">HR Portal</h1>
          <p className="text-sm text-muted-foreground">
            Sign in with your organization account to continue.
          </p>
        </div>
        <Button className="w-full" onClick={handleLogin} disabled={isLoading}>
          Sign in
        </Button>
      </Card>
    </div>
  );
}
