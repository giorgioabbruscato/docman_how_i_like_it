import { Link, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/stores/auth-store';
import { Button } from '@/components/ui/button';
import { LayoutDashboard, Building2, Users } from 'lucide-react';

const navItems = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/departments', label: 'Departments', icon: Building2 },
  { to: '/employees', label: 'Employees', icon: Users },
];

export function AppLayout() {
  const { user, logout } = useAuthStore();

  return (
    <div className="min-h-screen flex">
      <aside className="w-64 border-r border-border bg-muted/30 p-4">
        <div className="mb-8">
          <h1 className="text-xl font-bold">HR Portal</h1>
          <p className="text-sm text-muted-foreground">Multi-tenant platform</p>
        </div>
        <nav className="space-y-1">
          {navItems.map(({ to, label, icon: Icon }) => (
            <Link
              key={to}
              to={to}
              className="flex items-center gap-2 rounded-md px-3 py-2 text-sm hover:bg-muted"
            >
              <Icon className="h-4 w-4" />
              {label}
            </Link>
          ))}
        </nav>
      </aside>

      <div className="flex-1 flex flex-col">
        <header className="h-14 border-b border-border flex items-center justify-between px-6">
          <span className="text-sm text-muted-foreground">
            Tenant: <strong>{import.meta.env.VITE_TENANT_ID ?? 'demo'}</strong>
          </span>
          <div className="flex items-center gap-3">
            {user ? (
              <>
                <span className="text-sm">{user.name}</span>
                <Button variant="outline" size="sm" onClick={logout}>
                  Logout
                </Button>
              </>
            ) : (
              <span className="text-sm text-muted-foreground">Not authenticated</span>
            )}
          </div>
        </header>
        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
