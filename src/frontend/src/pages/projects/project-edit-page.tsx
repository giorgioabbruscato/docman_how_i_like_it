import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ProjectForm } from '@/components/projects/project-form';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { useProject, useUpdateProject } from '@/hooks/use-projects';
import { Permission, hasPermission } from '@/lib/auth-permissions';
import { getApiErrorMessage } from '@/lib/utils';
import { useAuthStore } from '@/stores/auth-store';
import type { CreateProjectRequest, UpdateProjectRequest } from '@/types/project';

export function ProjectEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const permissions = useAuthStore((state) => state.permissions);
  const canUpdate = hasPermission(permissions, Permission.ProjectUpdateTenant);
  const { data: project, isLoading, error } = useProject(id);
  const updateProject = useUpdateProject();
  const [formError, setFormError] = useState<string | null>(null);

  if (!canUpdate) {
    return (
      <div className="space-y-6">
        <h2 className="text-3xl font-bold tracking-tight">Edit Project</h2>
        <p className="text-muted-foreground">You do not have permission to edit projects.</p>
      </div>
    );
  }

  if (isLoading) {
    return <LoadingSpinner label="Loading project" />;
  }

  if (error || !project) {
    return <ErrorBanner message={getApiErrorMessage(error, 'Project not found.')} />;
  }

  const handleSubmit = async (data: CreateProjectRequest) => {
    if (!id) return;
    const input: UpdateProjectRequest = {
      ...data,
      status: data.status ?? project.status,
    };
    try {
      setFormError(null);
      await updateProject.mutateAsync({ id, input });
      navigate(`/projects/${id}`);
    } catch (err) {
      setFormError(getApiErrorMessage(err, 'Failed to update project.'));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Edit Project</h2>
          <p className="text-muted-foreground">{project.name}</p>
        </div>
        <Link to={`/projects/${project.id}`}>
          <Button type="button" variant="outline">Cancel</Button>
        </Link>
      </div>

      {formError && <ErrorBanner message={formError} />}

      <Card>
        <CardHeader>
          <CardTitle>Project Details</CardTitle>
        </CardHeader>
        <CardContent>
          <ProjectForm project={project} onSubmit={handleSubmit} submitLabel="Save Changes" />
        </CardContent>
      </Card>
    </div>
  );
}
