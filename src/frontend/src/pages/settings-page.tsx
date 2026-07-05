import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { isSingleTenancyMode } from '@/lib/tenancy-config';
import { useAuthStore } from '@/stores/auth-store';

export function SettingsPage() {
  const user = useAuthStore((state) => state.user);
  const me = useAuthStore((state) => state.me);
  const tenantDisplay = me?.tenantSlug ?? import.meta.env.VITE_TENANT_ID ?? 'demo';

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Settings</h2>
        <p className="text-muted-foreground">Your profile and account information.</p>
      </div>

      <Card className="max-w-lg">
        <CardHeader>
          <CardTitle>Profile</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <p className="text-sm text-muted-foreground">Name</p>
            <p className="font-medium">{user?.name ?? '—'}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Email</p>
            <p className="font-medium">{user?.email ?? '—'}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Roles</p>
            <p className="font-medium">
              {user?.roles.length ? user.roles.join(', ') : 'No roles assigned'}
            </p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Organization</p>
            <p className="font-medium">
              {isSingleTenancyMode ? 'Single organization' : tenantDisplay}
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
