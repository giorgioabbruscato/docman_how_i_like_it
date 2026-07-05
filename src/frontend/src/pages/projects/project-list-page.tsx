import { Link } from 'react-router-dom';
import { ProjectFilters } from '@/components/projects/project-filters';
import { ProjectList } from '@/components/projects/project-list';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner } from '@/components/ui/loading-spinner';
import { useProjects } from '@/hooks/use-projects';
import { Permission, hasPermission } from '@/lib/auth-permissions';
import { getApiErrorMessage } from '@/lib/utils';
import { useProjectUiStore } from '@/stores/project-ui-store';
import { useAuthStore } from '@/stores/auth-store';

const PAGE_SIZE = 20;

export function ProjectListPage() {
  const permissions = useAuthStore((state) => state.permissions);
  const canRead = hasPermission(permissions, Permission.ProjectReadTenant);
  const canCreate = hasPermission(permissions, Permission.ProjectCreateTenant);

  const {
    search,
    customerName,
    status,
    isArchived,
    page,
    setSearch,
    setCustomerName,
    setStatus,
    setIsArchived,
    setPage,
    resetFilters,
  } = useProjectUiStore();

  const { data, isLoading, error } = useProjects({
    page,
    pageSize: PAGE_SIZE,
    search: search || undefined,
    customerName: customerName || undefined,
    status: status || undefined,
    isArchived: isArchived || undefined,
  });

  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / PAGE_SIZE));

  const applyFilters = () => {
    setPage(1);
  };

  const handleReset = () => {
    resetFilters();
  };

  if (!canRead) {
    return (
      <div className="space-y-6">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Projects</h2>
          <p className="text-muted-foreground">
            You do not have permission to view projects. The project.read:tenant permission is
            required.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Projects</h2>
          <p className="text-muted-foreground">Manage projects and team assignments.</p>
        </div>
        {canCreate && (
          <Link to="/projects/new">
            <Button type="button">New Project</Button>
          </Link>
        )}
      </div>

      {error && (
        <ErrorBanner message={getApiErrorMessage(error, 'Failed to load projects.')} />
      )}

      <ProjectFilters
        search={search}
        customerName={customerName}
        status={status}
        isArchived={isArchived}
        onSearchChange={setSearch}
        onCustomerNameChange={setCustomerName}
        onStatusChange={setStatus}
        onIsArchivedChange={setIsArchived}
        onApply={applyFilters}
        onReset={handleReset}
      />

      <Card>
        <CardHeader>
          <CardTitle>All Projects</CardTitle>
        </CardHeader>
        <CardContent>
          <ProjectList
            projects={data?.items ?? []}
            loading={isLoading}
            page={page}
            totalPages={totalPages}
            totalCount={data?.totalCount ?? 0}
            onPageChange={setPage}
          />
        </CardContent>
      </Card>
    </div>
  );
}
