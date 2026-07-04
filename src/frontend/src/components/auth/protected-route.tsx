import { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { keycloak } from '@/lib/keycloak';
import { useAuth } from '@/providers/auth-provider';

function RouteLoadingScreen() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div
        className="h-8 w-8 animate-spin rounded-full border-2 border-primary border-t-transparent"
        role="status"
        aria-label="Loading"
      />
    </div>
  );
}

export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      void keycloak.login({ redirectUri: window.location.href });
    }
  }, [isAuthenticated, isLoading]);

  if (isLoading) {
    return <RouteLoadingScreen />;
  }

  if (!isAuthenticated) {
    return <RouteLoadingScreen />;
  }

  return <Outlet />;
}
