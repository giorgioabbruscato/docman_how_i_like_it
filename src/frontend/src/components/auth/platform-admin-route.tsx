import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/stores/auth-store';

export function PlatformAdminRoute() {
  const isPlatformAdmin = useAuthStore((state) => state.isPlatformAdmin);

  if (!isPlatformAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
}
