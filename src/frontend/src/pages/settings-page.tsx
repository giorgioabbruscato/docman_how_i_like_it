import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  disconnectCalendar,
  fetchCalendarConnections,
  fetchCalendarProviders,
  getCalendarConnectUrl,
} from '@/api/calendarIntegrations';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import { isSingleTenancyMode } from '@/lib/tenancy-config';
import { useAuthStore } from '@/stores/auth-store';

const CALLBACK_PATH = '/settings/calendar/callback';

export function SettingsPage() {
  const user = useAuthStore((state) => state.user);
  const me = useAuthStore((state) => state.me);
  const tenantDisplay = me?.tenantSlug ?? import.meta.env.VITE_TENANT_ID ?? 'demo';
  const canConnect = useHasPermission(Permission.CalendarConnectSelf);
  const queryClient = useQueryClient();

  const { data: providers, isLoading: providersLoading } = useQuery({
    queryKey: ['calendar-providers'],
    queryFn: fetchCalendarProviders,
    enabled: canConnect,
  });

  const { data: connections, isLoading: connectionsLoading } = useQuery({
    queryKey: ['calendar-connections'],
    queryFn: fetchCalendarConnections,
    enabled: canConnect,
  });

  const disconnectMutation = useMutation({
    mutationFn: disconnectCalendar,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['calendar-connections'] }),
  });

  const connectMutation = useMutation({
    mutationFn: async (provider: string) => {
      const redirectUri = `${window.location.origin}${CALLBACK_PATH}`;
      const { authorizationUrl } = await getCalendarConnectUrl(provider, redirectUri);
      window.location.href = authorizationUrl;
    },
  });

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

      {canConnect && (
        <Card>
          <CardHeader>
            <CardTitle>External calendars</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-sm text-muted-foreground">
              Connect Google Calendar or Microsoft 365 to sync approved leave automatically.
            </p>

            {providersLoading ? (
              <LoadingSpinner />
            ) : (
              <div className="flex flex-wrap gap-2">
                {providers?.map((provider) => (
                  <Button
                    key={provider.id}
                    variant="outline"
                    size="sm"
                    disabled={connectMutation.isPending}
                    onClick={() => void connectMutation.mutateAsync(provider.id)}
                  >
                    Connect {provider.name}
                  </Button>
                ))}
              </div>
            )}

            {connectionsLoading ? (
              <LoadingSpinner />
            ) : connections && connections.length > 0 ? (
              <ul className="space-y-2">
                {connections.map((connection) => (
                  <li
                    key={connection.id}
                    className="flex items-center justify-between rounded-md border px-3 py-2 text-sm"
                  >
                    <span>
                      {connection.provider} — connected{' '}
                      {new Date(connection.connectedAt).toLocaleDateString()}
                    </span>
                    <Button
                      variant="ghost"
                      size="sm"
                      disabled={disconnectMutation.isPending}
                      onClick={() => void disconnectMutation.mutateAsync(connection.id)}
                    >
                      Disconnect
                    </Button>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-muted-foreground">No calendars connected yet.</p>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
