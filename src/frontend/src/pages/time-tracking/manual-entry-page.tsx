import { Link } from 'react-router-dom';
import { ManualEntryForm } from '@/components/time-tracking/manual-entry-form';
import { Button } from '@/components/ui/button';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { useAuthStore } from '@/stores/auth-store';

export function ManualEntryPage() {
  const permissions = useAuthStore((state) => state.permissions);
  const canCreate = hasAnyPermission(permissions, Permission.TimeEntryCreateSelf);

  if (!canCreate) {
    return (
      <div className="space-y-6">
        <h2 className="text-3xl font-bold tracking-tight">Manual Entry</h2>
        <p className="text-muted-foreground">You do not have permission to create time entries.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Manual Entry</h2>
          <p className="text-muted-foreground">Log worked hours manually.</p>
        </div>
        <Link to="/time-tracking">
          <Button type="button" variant="outline">Back</Button>
        </Link>
      </div>
      <ManualEntryForm />
    </div>
  );
}
