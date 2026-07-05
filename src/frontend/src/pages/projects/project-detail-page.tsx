import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ProjectMemberList } from '@/components/projects/project-member-list';
import { ProjectStatusBadge } from '@/components/projects/project-status-badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { useDeleteProject, useProject } from '@/hooks/use-projects';
import { Permission, hasPermission } from '@/lib/auth-permissions';
import { confirmAction, formatDate, getApiErrorMessage } from '@/lib/utils';
import { useProjectUiStore } from '@/stores/project-ui-store';
import { useAuthStore } from '@/stores/auth-store';

export function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const permissions = useAuthStore((state) => state.permissions);
  const { activeTab, setActiveTab } = useProjectUiStore();
  const { data: project, isLoading, error } = useProject(id);
  const deleteProject = useDeleteProject();
  const [actionError, setActionError] = useState<string | null>(null);

  const canRead = hasPermission(permissions, Permission.ProjectReadTenant);
  const canUpdate = hasPermission(permissions, Permission.ProjectUpdateTenant);
  const canDelete = hasPermission(permissions, Permission.ProjectDeleteTenant);
  const canManageMembers = hasPermission(permissions, Permission.ProjectManageMembersTenant);

  if (!canRead) {
    return (
      <div className="space-y-6">
        <h2 className="text-3xl font-bold tracking-tight">Project Details</h2>
        <p className="text-muted-foreground">You do not have permission to view projects.</p>
      </div>
    );
  }

  const handleDelete = async () => {
    if (!id || !confirmAction('Archive this project?')) return;
    try {
      setActionError(null);
      await deleteProject.mutateAsync(id);
      navigate('/projects');
    } catch (err) {
      setActionError(getApiErrorMessage(err, 'Failed to archive project.'));
    }
  };

  if (isLoading) {
    return <LoadingSpinner label="Loading project" />;
  }

  if (error || !project) {
    return (
      <ErrorBanner message={getApiErrorMessage(error, 'Project not found.')} />
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h2 className="text-3xl font-bold tracking-tight">{project.name}</h2>
            <ProjectStatusBadge status={project.status} />
          </div>
          <p className="text-muted-foreground">{project.customerName ?? 'No customer'}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link to="/projects">
            <Button type="button" variant="outline">Back</Button>
          </Link>
          {canUpdate && (
            <Link to={`/projects/${project.id}/edit`}>
              <Button type="button" variant="outline">Edit</Button>
            </Link>
          )}
          {canDelete && !project.isArchived && (
            <Button variant="outline" onClick={() => void handleDelete()} disabled={deleteProject.isPending}>
              Archive
            </Button>
          )}
        </div>
      </div>

      {actionError && <ErrorBanner message={actionError} />}

      <div className="flex gap-2 border-b border-border">
        <button
          type="button"
          className={`px-4 py-2 text-sm ${activeTab === 'info' ? 'border-b-2 border-primary font-medium' : 'text-muted-foreground'}`}
          onClick={() => setActiveTab('info')}
        >
          Info
        </button>
        <button
          type="button"
          className={`px-4 py-2 text-sm ${activeTab === 'members' ? 'border-b-2 border-primary font-medium' : 'text-muted-foreground'}`}
          onClick={() => setActiveTab('members')}
        >
          Members
        </button>
      </div>

      {activeTab === 'info' ? (
        <Card>
          <CardHeader>
            <CardTitle>Project Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3 text-sm">
            {project.description && <p>{project.description}</p>}
            <dl className="grid gap-3 sm:grid-cols-2">
              <div>
                <dt className="text-muted-foreground">Start date</dt>
                <dd>{project.startDate ? formatDate(project.startDate) : '—'}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">End date</dt>
                <dd>{project.endDate ? formatDate(project.endDate) : '—'}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">Budget hours</dt>
                <dd>{project.budgetHours ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">Budget cost</dt>
                <dd>{project.budgetCost ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">Archived</dt>
                <dd>{project.isArchived ? 'Yes' : 'No'}</dd>
              </div>
            </dl>
          </CardContent>
        </Card>
      ) : (
        <ProjectMemberList projectId={project.id} canManageMembers={canManageMembers} />
      )}
    </div>
  );
}
